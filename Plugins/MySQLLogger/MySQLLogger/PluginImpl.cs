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

        public void SessionChange(System.ServiceProcess.SessionChangeDescription changeDescription)
        {
            m_logger.Debug("SessionChange()");

            string userName = GetUserName(changeDescription.SessionId);
            string msg = null;
            bool okToLog = false;

            switch (changeDescription.Reason)
            {
                case System.ServiceProcess.SessionChangeReason.SessionLogon:
                    okToLog = Settings.Store.EvtLogon;
                    if( okToLog )
                        msg = string.Format("Logon user={0} session id={1}", userName, changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionLogoff:
                    okToLog = Settings.Store.EvtLogoff;
                    if( okToLog )
                        msg = string.Format("Logoff user={0} session id={1}", userName, changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionLock:
                    okToLog = Settings.Store.EvtLock;
                    if( okToLog )
                        msg = string.Format("Session lock user={0} session id={1}", userName, changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionUnlock:
                    okToLog = Settings.Store.EvtUnlock;
                    if( okToLog )
                        msg = string.Format("Session unlock user={0} session id={1}", userName, changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionRemoteControl:
                    okToLog = Settings.Store.EvtRemoteControl;
                    if( okToLog )
                        msg = string.Format("Remote control user={0} session id={1}", userName, changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.ConsoleConnect:
                    okToLog = Settings.Store.EvtConsoleConnect;
                    if( okToLog )
                        msg = string.Format("Console connect user={0} session id={1}", userName, changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.ConsoleDisconnect:
                    okToLog = Settings.Store.EvtConsoleDisconnect;
                    if( okToLog )
                        msg = string.Format("Console disconnect user={0} session id={1}", userName, changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.RemoteConnect:
                    okToLog = Settings.Store.EvtRemoteConnect;
                    if( okToLog )
                        msg = string.Format("Remote connect user={0} session id={1}", userName, changeDescription.SessionId);
                    break;
                case System.ServiceProcess.SessionChangeReason.RemoteDisconnect:
                    okToLog = Settings.Store.EvtRemoteDisconnect;
                    if( okToLog )
                        msg = string.Format("Remote disconnect user={0} session id={1}", userName, changeDescription.SessionId);
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
            
        }

        public void Stopping()
        {
            
        }

        private string GetUserName(int sessionId)
        {
            m_logger.DebugFormat("GetUserName({0})", sessionId);
            try
            {
                return pInvokes.GetUserName(sessionId);
            }
            catch (Win32Exception)
            {
                return "--";
            }
        }
    }
}
