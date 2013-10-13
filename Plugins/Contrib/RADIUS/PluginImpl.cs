/*
	Copyright (c) 2011, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors 
		  may be used to endorse or promote products derived from this software without 
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using pGina.Shared.Settings;


namespace pGina.Plugin.RADIUS
{

    //TODO: Keep track of sessionID, needs to be unique for each session, but be maintained through auth and accounting

    //Caled-Station-ID - MAC Addr
    //NAS-Identifier - Customizable

    //Acct-Interim-Interval - radius accting, sends accting updates every x seconds


    //Idle-Timeout - Unsure if possible / pgina's responsibility
    //Session-Timeout = 178518 - Still needs to work correctly
    //WISPr-Session-Terminate-Time := "2014-03-11T23:59:59"

    //Return server response message

    //(6:02:09 AM) emias: Oooska: Acct-Status-Type isn't set to "Start" or "Stop", but to some invalid values.  And the Acct-Unique-Session-Id isn't identical on login and logout.

    //Select network adapter?

    public class RADIUSPlugin : IPluginConfiguration, IPluginAuthentication, IPluginEventNotifications
    {
        private ILog m_logger = LogManager.GetLogger("RADIUSPlugin");
        public static Guid SimpleUuid = new Guid("{350047A0-2D0B-4E24-9F99-16CD18D6B142}");
        private string m_defaultDescription = "A RADIUS Authentication and Accounting Plugin";
        private dynamic m_settings = null;

        private Dictionary<string, string> m_acctingSessionIDs;
        private SessionLimiter m_sessionLimiter = null;
        private Timer m_sessionTimer = null;

        public RADIUSPlugin()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                m_settings = new pGinaDynamicSettings(SimpleUuid);
                m_settings.SetDefault("ShowDescription", true);
                m_settings.SetDefault("Description", m_defaultDescription);
                
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }

            
        }        

        public string Name
        {
            get { return "RADIUS Plugin"; }
        }

        public string Description
        {
            get { return m_settings.Description; }
        }

        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public Guid Uuid
        {
            get { return SimpleUuid; }
        }

        //Authenticates user
        BooleanResult IPluginAuthentication.AuthenticateUser(SessionProperties properties)
        {

            m_logger.DebugFormat("AuthenticateUser({0})", properties.Id.ToString());

            // Get user info
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            try
            {
                RADIUSClient client = GetClient(null); //No session ID is required for auth, only accounting
                bool result = client.Authenticate(userInfo.Username, userInfo.Password);
                if (result)
                {
                    if (m_settings.AllowSessionTimeout)
                    {
                        Packet p = client.lastReceievedPacket;
                        if (p.getAttributeTypes().Contains(Packet.AttributeType.Session_Timeout))
                        {   //We have a Session Timeout attribute. 
                            byte[] bTimeout = client.lastReceievedPacket.getRawAttribute(Packet.AttributeType.Session_Timeout);

                            int seconds = BitConverter.ToInt32(bTimeout, 0);
                            m_logger.DebugFormat("Setting timeout for {} to {} seconds.", userInfo.Username, seconds);

                            if (m_sessionTimer == null)
                            {
                                m_sessionTimer = new Timer(new TimerCallback(SessionLimiterCallback),
                                    null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(60));
                            }
                            //m_sessionLimiter.Add(properties.Id, seconds);

                        }
                    }
                    return new BooleanResult() { Success = result };
                }
                return new BooleanResult() { Success = result, Message = "Invalid username or password." };
            }
            catch (RADIUSException re)
            {
                m_logger.Error("An error occurred during while authenticating.", re);
                return new BooleanResult() { Success = false, Message = re.Message };
            }
            catch (Exception e)
            {
                m_logger.Error("An unexpected error occurred while authenticating.", e);
                throw e;
            }
        }

        //Processes accounting on logon/logoff
        public void SessionChange(System.ServiceProcess.SessionChangeDescription changeDescription, pGina.Shared.Types.SessionProperties properties)
        {
            m_logger.DebugFormat("SessionChange({0})", properties.Id.ToString());

            string username = null;
            if ((bool)Settings.Store.UseModifiedName)
                username = properties.GetTrackedSingle<UserInformation>().Username;
            else
                username = properties.GetTrackedSingle<UserInformation>().OriginalUsername;

            if (changeDescription.Reason == System.ServiceProcess.SessionChangeReason.SessionLogon)
            {
                //Create a new unique id for this accounting session and store it
                String sessionId = Guid.NewGuid().ToString();
                lock (m_acctingSessionIDs)
                {
                    m_acctingSessionIDs.Add(username, sessionId);
                }

                //Determine which plugin authenticated the user (if any)
                PluginActivityInformation pai = properties.GetTrackedSingle<PluginActivityInformation>();
                Packet.Acct_AuthenticType authSource = Packet.Acct_AuthenticType.Not_Specified;
                IEnumerable<Guid> authPlugins = pai.GetAuthenticationPlugins();
                Guid LocalMachinePluginGuid = new Guid("{12FA152D-A2E3-4C8D-9535-5DCD49DFCB6D}");
                foreach (Guid guid in authPlugins)
                {
                    if (pai.GetAuthenticationResult(guid).Success)
                    {
                        if (guid == SimpleUuid)
                            authSource = Packet.Acct_AuthenticType.RADIUS;
                        else if (guid == LocalMachinePluginGuid)
                            authSource = Packet.Acct_AuthenticType.Local;
                        else
                            authSource = Packet.Acct_AuthenticType.Remote;
                        break;
                    }
                }                

                try
                {
                    RADIUSClient client = GetClient(sessionId);
                    client.startAccounting(username, authSource);
                }
                catch (Exception e)
                {
                    m_logger.Error("Error occurred while starting accounting.", e);
                }

            }
            
            else if (changeDescription.Reason == System.ServiceProcess.SessionChangeReason.SessionLogoff)
            {
                //Check if guid was stored from accounting start request (if not, no point in sending a stop request)
                string sessionId = null;
                lock (m_acctingSessionIDs)
                {
                    sessionId = m_acctingSessionIDs.ContainsKey(username) ? m_acctingSessionIDs[username] : null;

                    if (sessionId == null)
                    {
                        m_logger.ErrorFormat("Error sending accounting stop request. No guid available for {0}", username);
                        return;
                    } //Remove the session id since we're logging off
                    m_acctingSessionIDs.Remove(username);
                }

                if (m_sessionLimiter != null)
                {
                    m_sessionLimiter.Remove(changeDescription.SessionId);
                    if (m_sessionLimiter.Count() == 0)
                    {
                        m_sessionTimer.Dispose();
                        m_sessionTimer = null;
                    }

                }

                try
                {
                    RADIUSClient client = GetClient(sessionId);
                    client.stopAccounting(username, Packet.Acct_Terminate_CauseType.User_Request);
                }
                catch (Exception e)
                {
                    m_logger.Error("Error occurred while stopping accounting.", e);
                    return;
                }
            }
        }

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }

        public void Starting() 
        { 
            m_acctingSessionIDs = new Dictionary<string, string>();
            
            if(m_settings.AllowSessionTimeout)
                m_sessionLimiter = new SessionLimiter();
        }
        public void Stopping() { }


        //Returns the client instantiated based on registry settings
        private RADIUSClient GetClient(string sessionId)
        {
            string[] servers = Regex.Split(Settings.Store.Server.Trim(), @"\s+");
            int authport = Settings.Store.AuthPort;
            int acctport = Settings.Store.AcctPort;
            string sharedKey = Settings.Store.GetEncryptedSetting("SharedSecret");
            int timeout = Settings.Store.Timeout;
            int retry = Settings.Store.Retry;


            byte[] ipAddr = null;
            string nasIdentifier = null;
            
            if((bool)m_settings.Store.SendNASIPAddress)
                ipAddr = getNetworkInfo().Item1;

            if((bool)m_settings.Store.SendNASIdentifier){
                nasIdentifier = (String)m_settings.Store.NASIdentifier;
                nasIdentifier = nasIdentifier.Contains('%') ? replaceSymbols(nasIdentifier) : nasIdentifier;
            }


 

            
            RADIUSClient client = new RADIUSClient(servers, authport, acctport, sharedKey, timeout, retry, sessionId, ipAddr, machineName);


            return client;
        }

        private string replaceSymbols(string str)
        {
            Tuple<byte[], string> networkInfo = getNetworkInfo();
            return str.Replace("%macaddr", networkInfo.Item2)
                .Replace("%ipaddr", String.Join(".", networkInfo.Item1))
                .Replace("%computername", Environment.MachineName);
        }
        
        //Returns a tuple containing the current IPv4 address and mac address for the adapter
        //If ipAddressRegex is set, this will attempt to return the first address that matches the expression
        //Otherwise it returns the first viable IP address or 0.0.0.0 if no viable address is found. An empty
        //string is sent if no mac address is determined.
        private Tuple<byte[], string> getNetworkInfo()
        {
            string ipAddressRegex = Settings.Store.IPSuggestion;
           
            //Fallback values
            byte[] ipAddr = null;
            string macAddr = null;

            //Check each network adapter. 
            foreach(NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()){
                foreach (UnicastIPAddressInformation ipaddr in nic.GetIPProperties().UnicastAddresses)
                {   //Check to see if the NIC has any valid IP addresses.
                    if (ipaddr.Address.AddressFamily == AddressFamily.InterNetwork)
                        if (String.IsNullOrEmpty(ipAddressRegex) || //IP address, grab first adapter or check if it matches ip regex
                          Regex.Match(ipaddr.Address.ToString(), ipAddressRegex).Success)
                            return Tuple.Create(ipaddr.Address.GetAddressBytes(), nic.GetPhysicalAddress().ToString());
                        else if(ipAddr == null && macAddr == null){ //Fallback, grab info from first device
                            ipAddr = ipaddr.Address.GetAddressBytes();
                            macAddr = nic.GetPhysicalAddress().ToString();
                        }
                }
            }
            if (ipAddr == null) ipAddr = new byte[] { 0, 0, 0, 0 };
            if (macAddr == null) macAddr = "";
            return Tuple.Create(ipAddr, macAddr);
        }

        private void SessionLimiterCallback(object state)
        {
            foreach(int sess in m_sessionLimiter.ExpiredSessions()){
                m_logger.DebugFormat("Logging off session{0}.", sess);
                bool result = Abstractions.WindowsApi.pInvokes.LogoffSession(sess);
                m_logger.DebugFormat("Log off {0}.", result ? "successful" : "failed");
            }
        }
    }
}
