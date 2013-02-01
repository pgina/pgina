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
using System.Diagnostics;
using System.Threading;

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using pGina.Shared.Settings;
using Abstractions.WindowsApi;

namespace pGina.Plugin.SessionLimit
{
    public class PluginImpl : IPluginConfiguration, IPluginEventNotifications
    {
        private ILog m_logger = LogManager.GetLogger("SessionLimitPlugin");
        public static Guid PluginUuid = new Guid("{D73131D7-7AF2-47BB-BBF4-4F8583B44962}");

        private Timer m_timer;
        private SessionCache m_cache;

        public PluginImpl()
        {
            using (Process me = Process.GetCurrentProcess())
            {
                Settings.Init();
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }

        private void StartTimer()
        {
            m_logger.Debug("Starting timer");
            m_timer = new Timer(new TimerCallback(SessionLimitTimerCallback), null, TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(60));
        }

        private void StopTimer()
        {
            m_logger.Debug("Stopping timer");
            m_timer.Dispose();
            m_timer = null;
        }

        private void SessionLimitTimerCallback(object state)
        {
            int limit = Settings.Store.GlobalLimit;

            if (limit > 0)
            {
                m_logger.Debug("Checking for sessions to logoff");
                List<int> sessions = m_cache.SessionsLoggedOnLongerThan(TimeSpan.FromMinutes(limit));
                m_logger.DebugFormat("Found {0} sessions.", sessions.Count);
                foreach (int sess in sessions)
                {
                    m_logger.DebugFormat("Logging off session {0}", sess);
                    bool result = Abstractions.WindowsApi.pInvokes.LogoffSession(sess);
                    if (result)
                        m_logger.Debug("Log off successful.");
                    else
                        m_logger.Debug("Log off failed.");
                }
            }
        }

        public string Name
        {
            get { return "Session Limit"; }
        }

        public string Description
        {
            get { return "Enforces limits to user's sessions"; }
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
            get { return PluginUuid; }
        }

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }

        public void Starting() 
        {
            m_cache = new SessionCache();
            StartTimer();
        }

        public void Stopping() 
        { 
            StopTimer();
            m_cache = null;
        }

        public void SessionChange(System.ServiceProcess.SessionChangeDescription changeDescription, SessionProperties properties)
        {
            // Only applies to pGina sessions!
            if (properties != null)
            {
                switch (changeDescription.Reason)
                {
                    case System.ServiceProcess.SessionChangeReason.SessionLogon:
                        LogonEvent(changeDescription.SessionId);
                        break;
                    case System.ServiceProcess.SessionChangeReason.SessionLogoff:
                        LogoffEvent(changeDescription.SessionId);
                        break;
                }
            }
        }

        private void LogonEvent(int sessId)
        {
            m_logger.DebugFormat("LogonEvent: {0}", sessId);
            m_cache.Add(sessId);
        }

        private void LogoffEvent(int sessId)
        {
            m_logger.DebugFormat("LogoffEvent: {0}", sessId);
            m_cache.Remove(sessId);
        }
    }
}
