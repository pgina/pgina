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

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using pGina.Shared.Settings;


namespace pGina.Plugin.RADIUS
{

    //TODO: Keep track of sessionID, needs to be unique for each session, but be maintained through auth and accounting

    enum MachineIdentifier { IP_Address = 0, Machine_Name, Both}

    public class RADIUSPlugin : IPluginConfiguration, IPluginAuthentication, IPluginEventNotifications
    {
        private ILog m_logger = LogManager.GetLogger("RADIUSPlugin");
        public static Guid SimpleUuid = new Guid("{350047A0-2D0B-4E24-9F99-16CD18D6B142}");
        private string m_defaultDescription = "A RADIUS Authentication and Accounting Plugin";
        private dynamic m_settings = null;

        private Dictionary<string, string> sessionIDs;
        private Object sessionIDLock;

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
                    return new BooleanResult() { Success = result };
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
                lock (sessionIDLock)
                {
                    sessionIDs.Add(username, sessionId);
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
                lock (sessionIDLock)
                {
                    sessionId = sessionIDs.ContainsKey(username) ? sessionIDs[username] : null;

                    if (sessionId == null)
                    {
                        m_logger.ErrorFormat("Error sending accounting stop request. No guid available for {0}", username);
                        return;
                    } //Remove the session id since we're logging off
                    sessionIDs.Remove(username);
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
            sessionIDs = new Dictionary<string, string>();
            sessionIDLock = new Object();
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
                
            MachineIdentifier mid = (MachineIdentifier)((int)Settings.Store.MachineIdentifier);
            byte[] ipAddr = null;
            string machineName = null;

            if(mid == MachineIdentifier.Machine_Name || mid == MachineIdentifier.Both)
                machineName = Environment.MachineName;
            if(mid == MachineIdentifier.IP_Address || mid == MachineIdentifier.Both)
                ipAddr = getIPAddress();

            
            RADIUSClient client = new RADIUSClient(servers, authport, acctport, sharedKey, timeout, retry, sessionId, ipAddr, machineName);


            return client;
        }
        
        //Returns the current IPv4 address
        //If ipAddressRegex is set, this will attempt to return the first address that matches the expression
        //Otherwise it returns the first viable IP address or 0.0.0.0 if no viable address is found
        private byte[] getIPAddress()
        {
            string ipAddressRegex = Settings.Store.IPSuggestion;
            IPAddress[] ipList = Dns.GetHostAddresses("");
            IPAddress fallback = null;
            // Grab the first IPv4 address in the list
            foreach (IPAddress addr in ipList)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (fallback == null)
                        fallback = addr; //Grab the first viable IP address as a fallback
                    if (ipAddressRegex != null && Regex.Match(addr.ToString(), ipAddressRegex).Success)
                        return addr.GetAddressBytes();
                }
            }

            if (fallback != null)
                return fallback.GetAddressBytes();
            return new byte[] { 0, 0, 0, 0 };
        }
    }
}
