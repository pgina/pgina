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

    //TODO:
    //Idle-Timeout - Unsure if possible / pgina's responsibility


    public class RADIUSPlugin : IPluginConfiguration, IPluginAuthentication, IPluginEventNotifications
    {
        private ILog m_logger = LogManager.GetLogger("RADIUSPlugin");
        public static Guid SimpleUuid = new Guid("{350047A0-2D0B-4E24-9F99-16CD18D6B142}");
        private string m_defaultDescription = "A RADIUS Authentication and Accounting Plugin";
        private dynamic m_settings = null;
        private Dictionary<Guid, Session> m_sessionManager;

        public RADIUSPlugin()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                m_settings = new pGinaDynamicSettings(SimpleUuid);
                m_settings.SetDefault("ShowDescription", true);
                m_settings.SetDefault("Description", m_defaultDescription);

                m_sessionManager = new Dictionary<Guid, Session>();
                
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

            if (!(bool)Settings.Store.EnableAuth)
            {
                m_logger.Debug("Authentication stage set on RADIUS plugin but authentication is not enabled in plugin settings.");
                return new BooleanResult() { Success = false };
            }

            // Get user info
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            if(String.IsNullOrEmpty(userInfo.Username) || String.IsNullOrEmpty(userInfo.Password))
                return new BooleanResult() { Success = false, Message = "Username and password must be provided." };

            try
            {
                RADIUSClient client = GetClient(); 
                bool result = client.Authenticate(userInfo.Username, userInfo.Password);
                if (result)
                {
                    Session session = new Session(properties.Id, userInfo.Username, client);
                    Packet p = client.lastReceievedPacket;

                    //Check for session timeout
                    if ((bool)Settings.Store.AllowSessionTimeout && p.containsAttribute(Packet.AttributeType.Session_Timeout))
                    {   
                        int seconds = client.lastReceievedPacket.getFirstIntAttribute(Packet.AttributeType.Session_Timeout);
                        session.SetSessionTimeout(seconds, SessionTimeoutCallback);
                        //m_logger.DebugFormat("Setting timeout for {0} to {1} seconds.", userInfo.Username, seconds);
                    }

                    if (p.containsAttribute(Packet.AttributeType.Idle_Timeout))
                    {
                        int seconds = client.lastReceievedPacket.getFirstIntAttribute(Packet.AttributeType.Idle_Timeout);
                    }

                    if(p.containsAttribute(Packet.AttributeType.Vendor_Specific)){
                        foreach(byte[] val in p.getByteArrayAttributes(Packet.AttributeType.Vendor_Specific)){
                            //m_logger.DebugFormat("Vendor ID: {0:D}, Type: {1:D}, Value: {2}", Packet.VSA_vendorID(val), Packet.VSA_VendorType(val), Packet.VSA_valueAsString(val));

                            if ((bool)Settings.Store.WisprSessionTerminate && Packet.VSA_vendorID(val) == (int)Packet.VSA_WISPr.Vendor_ID 
                                && Packet.VSA_VendorType(val) == (int)Packet.VSA_WISPr.WISPr_Session_Terminate_Time)
                            {
                                
                                try
                                {
                                    //Value is in format "2014-03-11T23:59:59"
                                    string sdt = Packet.VSA_valueAsString(val);
                                    DateTime dt = DateTime.ParseExact(sdt, "yyyy-MM-dd'T'HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                                    if (dt > DateTime.Now)
                                    {
                                        session.Set_Session_Terminate(dt, SessionTerminateCallback);
                                    }

                                    else
                                        m_logger.DebugFormat("The timestamp provided for WisperSessionTerminate time value has passed.");

                                }
                                catch (FormatException e)
                                {
                                    m_logger.DebugFormat("Unable to parse timestamp: {0}", Packet.VSA_valueAsString(val));
                                }
                            }
                        }

                    }

                    //Check for interim-update
                    if ((bool)Settings.Store.SendInterimUpdates)
                    {
                        int seconds = 0;

                        if (p.containsAttribute(Packet.AttributeType.Acct_Interim_Interval))
                        {
                            seconds = client.lastReceievedPacket.getFirstIntAttribute(Packet.AttributeType.Acct_Interim_Interval);
                        }

                        //Check to see if plugin is set to send interim updates more frequently
                        if ((bool)Settings.Store.ForceInterimUpdates)
                        {
                            int forceTime = (int)Settings.Store.InterimUpdateTime;
                            if (forceTime > 0)
                                seconds = forceTime;
                        }

                        //Set interim update
                        if (seconds > 0)
                        {
                            session.SetInterimUpdate(seconds, InterimUpdatesCallback);
                            m_logger.DebugFormat("Setting interim update interval for {0} to {1} seconds.", userInfo.Username, seconds);
                        }

                        else
                        {
                            m_logger.DebugFormat("Interim Updates are enabled, but no update interval was provided by the server or user.");
                        }
                        
                    }

                    lock (m_sessionManager)
                    {
                        //m_logger.DebugFormat("Adding session to m_sessionManager. ID: {0}, session: {1}", session.id, session);
                        m_sessionManager.Add(session.id, session);
                    }

                    string message = null;
                    if (p.containsAttribute(Packet.AttributeType.Reply_Message))
                        message = p.getFirstStringAttribute(Packet.AttributeType.Reply_Message);

                    return new BooleanResult() { Success = result, Message = message };
                }

                //Failure
                string msg = "Unable to validate username or password.";

                if (client.lastReceievedPacket == null)
                {
                    msg = msg + " No response from server.";
                }

                else if (client.lastReceievedPacket.containsAttribute(Packet.AttributeType.Reply_Message))
                {
                    msg = client.lastReceievedPacket.getFirstStringAttribute(Packet.AttributeType.Reply_Message);
                }

                else if (client.lastReceievedPacket.code == Packet.Code.Access_Reject)
                {
                    msg = msg + String.Format(" Access Rejected.");
                }

                return new BooleanResult() { Success = result, Message = msg };
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
            if (changeDescription.Reason != System.ServiceProcess.SessionChangeReason.SessionLogon
                && changeDescription.Reason != System.ServiceProcess.SessionChangeReason.SessionLogoff)
            {
                //m_logger.DebugFormat("Not logging on or off for this session change call ({0})... exiting.", changeDescription.Reason);
                return;
            }

            if (properties == null)
            {
                //m_logger.DebugFormat("No session properties available. This account does not appear to be managed by pGina. Exiting SessionChange()");
                return;
            }

            if (!(bool)Settings.Store.EnableAcct)
            {
                m_logger.Debug("Session Change stage set on RADIUS plugin but accounting is not enabled in plugin settings.");
                return;
            }

            //Determine username (may change depending on value of UseModifiedName setting)
            string username = null;
            UserInformation ui = properties.GetTrackedSingle<UserInformation>();

            if (ui == null)
            {
                //m_logger.DebugFormat("No userinformation for this session logoff... exiting...");
                return;
            }
                    

            if ((bool)Settings.Store.UseModifiedName)
                username = ui.Username;
            else
                username = ui.OriginalUsername;

            Session session = null;

            //User is logging on
            if (changeDescription.Reason == System.ServiceProcess.SessionChangeReason.SessionLogon)
            {
                lock (m_sessionManager)
                {
                    //Check if session information is already available for this id
                    if (!m_sessionManager.Keys.Contains(properties.Id))
                    {
                        //No session info - must have authed with something other than RADIUS.
                        //m_logger.DebugFormat("RADIUS Accounting Logon: Unable to find session for {0} with GUID {1}", username, properties.Id);

                        if(!(bool)Settings.Store.AcctingForAllUsers){
                            //m_logger.Debug("Accounting for non-RADIUS users is disabled. Exiting.");
                            return;
                        }

                        RADIUSClient client = GetClient();
                        session = new Session(properties.Id, username, client);
                        m_sessionManager.Add(properties.Id, session);
                            
                        //Check forced interim-update setting
                        if ((bool)Settings.Store.SendInterimUpdates && (bool)Settings.Store.ForceInterimUpdates)
                        {
                            int interval = (int)Settings.Store.InterimUpdateTime;
                            session.SetInterimUpdate(interval, InterimUpdatesCallback);
                        }
                    }

                    else
                        session = m_sessionManager[properties.Id];
                }


                //Determine which plugin authenticated the user (if any)
                PluginActivityInformation pai = properties.GetTrackedSingle<PluginActivityInformation>();
                Packet.Acct_Authentic authSource = Packet.Acct_Authentic.Not_Specified;
                IEnumerable<Guid> authPlugins = pai.GetAuthenticationPlugins();
                Guid LocalMachinePluginGuid = new Guid("{12FA152D-A2E3-4C8D-9535-5DCD49DFCB6D}");
                foreach (Guid guid in authPlugins)
                {
                    if (pai.GetAuthenticationResult(guid).Success)
                    {
                        if (guid == SimpleUuid)
                            authSource = Packet.Acct_Authentic.RADIUS;
                        else if (guid == LocalMachinePluginGuid)
                            authSource = Packet.Acct_Authentic.Local;
                        else //Not RADIUS, not Local, must be some other auth plugin
                            authSource = Packet.Acct_Authentic.Remote;
                        break;
                    }
                }

                //We can finally start the accounting process
                try
                {
                    lock (session)
                    {
                        session.windowsSessionId = changeDescription.SessionId; //Grab session ID now that we're authenticated
                        session.username = username; //Accting username may have changed depending on 'Use Modified username for accounting option'
                        session.client.startAccounting(username, authSource);
                        //m_logger.DebugFormat("Successfully completed accounting start process...");
                    }
                }
                catch (Exception e)
                {
                    m_logger.Error("Error occurred while starting accounting.", e);
                }

            }

                
            //User is logging off
            else if (changeDescription.Reason == System.ServiceProcess.SessionChangeReason.SessionLogoff)
            {
                lock (m_sessionManager)
                {
                    if (m_sessionManager.Keys.Contains(properties.Id))
                        session = m_sessionManager[properties.Id];
                    else
                    {
                        //m_logger.DebugFormat("Users {0} is logging off, but no RADIUS session information is available for session ID {1}.", username, properties.Id);
                        return;
                    }

                    //Remove the session from the session manager
                    m_sessionManager.Remove(properties.Id);
                }

                lock (session)
                {
                    //Disbale any active callbacks for this session
                    session.disableCallbacks();
                    session.active = false;

                    //Assume normal logout if no other terminate reason is listed.
                    if (session.terminate_cause == null)
                        session.terminate_cause = Packet.Acct_Terminate_Cause.User_Request;

                    try
                    {
                        //m_logger.DebugFormat("About to send accounting stop packet. Session has been active {0} seconds.", (DateTime.Now - session.client.accountingStartTime).TotalSeconds);
                        session.client.stopAccounting(session.username, session.terminate_cause);
                    }
                    catch (RADIUSException re)
                    {
                        m_logger.DebugFormat("Unable to send accounting stop message for user {0} with ID {1}. Message: {2}", session.username, session.id, re.Message);
                    }
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
            if(m_sessionManager == null)
                m_sessionManager = new Dictionary<Guid, Session>();
        }
        public void Stopping() { }


        //Returns the client instantiated based on registry settings
        private RADIUSClient GetClient(string sessionId = null)
        {
            string[] servers = Regex.Split(Settings.Store.Server.Trim(), @"\s+");
            int authport = Settings.Store.AuthPort;
            int acctport = Settings.Store.AcctPort;
            string sharedKey = Settings.Store.GetEncryptedSetting("SharedSecret");
            int timeout = Settings.Store.Timeout;
            int retry = Settings.Store.Retry;


            byte[] ipAddr = null;
            string nasIdentifier = null;
            string calledStationId = null;
            
            if((bool)Settings.Store.SendNASIPAddress)
                ipAddr = getNetworkInfo().Item1;

            if((bool)Settings.Store.SendNASIdentifier){
                nasIdentifier = Settings.Store.NASIdentifier;
                nasIdentifier = nasIdentifier.Contains('%') ? replaceSymbols(nasIdentifier) : nasIdentifier;
            }

            if ((bool)Settings.Store.SendCalledStationID)
            {
                calledStationId = (String)Settings.Store.CalledStationID;
                calledStationId = calledStationId.Contains('%') ? replaceSymbols(calledStationId) : calledStationId;
            }
 
            RADIUSClient client = new RADIUSClient(servers, authport, acctport, sharedKey, timeout, retry, sessionId, ipAddr, nasIdentifier, calledStationId);
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

        //Gets invoked by timer callback after the session times out
        private void SessionTimeoutCallback(object state)
        {
            Session session = (Session)state;

            //Lock session? Might cause issues when we call LogoffSession on user and trigger the SessionChange method?
            if(!session.windowsSessionId.HasValue){
                m_logger.DebugFormat("Attempting to log user {0} out due to timeout, but no windows session ID is present for ID {1}", session.username, session.id);
                return;
            }

            if (session.terminate_cause != null)
            {
                m_logger.DebugFormat("User {0} has timed out, but terminate cause #{1} has already been set for ID {2}", session.username, session.terminate_cause, session.id);
            }
            session.terminate_cause = Packet.Acct_Terminate_Cause.Session_Timeout;

            m_logger.DebugFormat("Logging off user {0} in session{1} due to session timeout.", session.username, session.windowsSessionId);
            bool result = Abstractions.WindowsApi.pInvokes.LogoffSession(session.windowsSessionId.Value);
            //m_logger.DebugFormat("Log off {0}.", result ? "successful" : "failed");
        }

        //Gets invoked if wispr session limit 
        private void SessionTerminateCallback(object state)
        {
            Session session = (Session)state;
            session.terminate_cause = Packet.Acct_Terminate_Cause.Session_Timeout;

            if (!session.windowsSessionId.HasValue)
            {
                m_logger.DebugFormat("Attempting to log user {0} out due to WISPr Session limit, but no windows session ID is present for ID {1}", session.username, session.id);
                return;
            }

            if (session.terminate_cause != null)
            {
                m_logger.DebugFormat("User {0} has reached WISPr Session limit, but terminate cause #{1} has already been set for ID {2}", session.username, session.terminate_cause, session.id);
            }
            session.terminate_cause = Packet.Acct_Terminate_Cause.Session_Timeout;

            m_logger.DebugFormat("Logging off user {0} in session{1} due to session-terminate-time.", session.username, session.windowsSessionId);
            bool result = Abstractions.WindowsApi.pInvokes.LogoffSession(session.windowsSessionId.Value);
            //m_logger.DebugFormat("Log off {0}.", result ? "successful" : "failed");
            
        }

        //Gets invoked when its time to send interim updates
        private void InterimUpdatesCallback(object state)
        {
            Session session = (Session)state;
            //m_logger.DebugFormat("Sending interim-update for user {0}", session.username); 
            lock (session)
            {
                try
                {
                    if (session.active)
                        session.client.interimUpdate(session.username);
                    else
                    {
                        session.disableCallbacks();
                    }
                }
                catch (RADIUSException e)
                {
                    m_logger.DebugFormat("Unable to send interim-update: {0}", e.Message);
                }
            }
        }
    }
}
