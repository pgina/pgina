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
using System.ComponentModel;

using pGina.Shared.Interfaces;
using pGina.Shared.Settings;
using Abstractions.WindowsApi;
using log4net;

namespace pGina.Plugin.MySqlLogger
{
    public class PluginImpl : IPluginConfiguration, IPluginEventNotifications
    {
        public static readonly Guid PluginUuid = new Guid("B68CF064-9299-4765-AC08-ACB49F93F892");
        private ILog m_logger = LogManager.GetLogger("MySqlLoggerPlugin");
        public static readonly string TABLE_NAME = "pGinaLog";
        private SessionCache m_usernameCache = new SessionCache();

        public static readonly string UNKNOWN_USERNAME = "--Unknown--";

        public string Description
        {
            get { return "Logs various events to a MySQL database."; }
        }

        public string Name
        {
            get { return "MySQL Logger"; }
        }

        public Guid Uuid
        {
            get { return PluginUuid; }
        }

        public string Version
        {
            get 
            { 
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); 
            }
        }

        public void Configure()
        {
            Configuration dlg = new Configuration();
            dlg.ShowDialog();
        }

        public void SessionChange(System.ServiceProcess.SessionChangeDescription changeDescription, pGina.Shared.Types.SessionProperties properties)
        {
            m_logger.DebugFormat("SessionChange({0})", changeDescription.Reason.ToString());

            string msg = null;

            switch (changeDescription.Reason)
            {
                case System.ServiceProcess.SessionChangeReason.SessionLogon:
                    msg = LogonEvent(changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionLogoff:
                    msg = LogoffEvent(changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionLock:
                    msg = SessionLockEvent(changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionUnlock:
                    msg = SessionUnlockEvent(changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionRemoteControl:
                    msg = SesionRemoteControlEvent(changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.ConsoleConnect:
                    msg = ConsoleConnectEvent(changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.ConsoleDisconnect:
                    msg = ConsoleDisconnectEvent(changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.RemoteConnect:
                    msg = RemoteConnectEvent(changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.RemoteDisconnect:
                    msg = RemoteDisconnectEvent(changeDescription.SessionId);
                    break;
            }

            if (!string.IsNullOrEmpty(msg))
            {
                m_logger.Debug(msg);

                // Log to DB
                try
                {
                    using (DbLogger log = DbLogger.Connect())
                    {
                        log.Log(msg);
                    }
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("Error logging to DB: {0}", e);
                }
            }
        }

        public void Starting()
        {
            this.m_usernameCache.Clear();
        }

        public void Stopping()
        {
            this.m_usernameCache.Clear();
        }

        private string LogonEvent(int sessionId)
        {
            bool okToLog = Settings.Store.EvtLogon;

            // Get the username
            string userName = SessionCache.TryHardToGetUserName(sessionId, 20, 500);

            // Since the username is not available at logoff time, we cache it
            // (tied to the session ID) so that we can get it back at the logoff
            // event.
            if (userName != null)
                m_usernameCache.Add(sessionId, userName);
            else
                userName = UNKNOWN_USERNAME;

            if (okToLog)
                return string.Format("[{0}] Logon user: {1}", sessionId, userName);

            return "";
        }

        private string LogoffEvent(int sessionId)
        {
            bool okToLog = Settings.Store.EvtLogoff;
            string userName = "";

            userName = m_usernameCache.Get(sessionId);
            // Delete the username from the cache because we are logging off?

            if (userName == null)
                userName = UNKNOWN_USERNAME;

            if (okToLog)
                return string.Format("[{0}] Logoff user: {1}", sessionId, userName);

            return "";
        }

        private string ConsoleConnectEvent(int sessionId)
        {
            bool okToLog = Settings.Store.EvtConsoleConnect;
            string userName = "";

            userName = this.m_usernameCache.Get(sessionId);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Console connect user: {1}", sessionId, userName);

            return "";
        }

        private string ConsoleDisconnectEvent(int sessionId)
        {
            bool okToLog = Settings.Store.EvtConsoleDisconnect;
            string userName = "";

            userName = this.m_usernameCache.Get(sessionId);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Console disconnect user: {1}", sessionId, userName);

            return "";
        }

        private string RemoteDisconnectEvent(int sessionId)
        {
            bool okToLog = Settings.Store.EvtRemoteDisconnect;
            string userName = "";

            userName = this.m_usernameCache.Get(sessionId);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Remote disconnect user: {1}", sessionId, userName);

            return "";
        }

        private string RemoteConnectEvent(int sessionId)
        {
            bool okToLog = Settings.Store.EvtRemoteConnect;
            string userName = "";

            userName = this.m_usernameCache.Get(sessionId);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Remote connect user: {1}", sessionId, userName);

            return "";
        }

        private string SesionRemoteControlEvent(int sessionId)
        {
            bool okToLog = Settings.Store.EvtRemoteControl;
            string userName = "";

            userName = this.m_usernameCache.Get(sessionId);
            if (userName == null)
                userName = UNKNOWN_USERNAME;

            
            if (okToLog)
                return string.Format("[{0}] Remote control user: {1}", sessionId, userName);

            return "";
        }

        private string SessionUnlockEvent(int sessionId)
        {
            bool okToLog = Settings.Store.EvtUnlock;
            string userName = "";

            userName = this.m_usernameCache.Get(sessionId);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Session unlock user: {1}", sessionId, userName);

            return "";
        }

        private string SessionLockEvent(int sessionId)
        {
            bool okToLog = Settings.Store.EvtLock;
            string userName = "";

            userName = this.m_usernameCache.Get(sessionId);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Session lock user: {1}", sessionId, userName);

            return "";
        }

    }
}
