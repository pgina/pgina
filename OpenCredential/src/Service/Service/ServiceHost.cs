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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

using OpenCredential.Service.Impl;

using log4net;

namespace Service
{
    public partial class OpenCredentialServiceHost : ServiceBase
    {
        private Thread m_serviceThread = null;
        private OpenCredential.Service.Impl.ServiceThread m_serviceThreadObj = null;

        public OpenCredentialServiceHost()
        {
            InitializeComponent();           
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                m_serviceThreadObj = new OpenCredential.Service.Impl.ServiceThread();
                m_serviceThread = new Thread(new ThreadStart(m_serviceThreadObj.Start));
                m_serviceThread.Start();

                // Wait briefly for a clean startup, but do not fail the Windows
                // service if initialization is merely slow. Some environments need
                // more time to load plugins, bring networking up, or complete the
                // first database health checks.
                bool initialized = m_serviceThreadObj.WaitForInitialization(15000);
                if (m_serviceThreadObj.StartupException != null)
                    throw new InvalidOperationException("OpenCredential service failed to initialize.", m_serviceThreadObj.StartupException);

                if (!initialized)
                {
                    EventLog.WriteEntry(
                        "OpenCredential",
                        "OpenCredential service initialization is taking longer than expected. Continuing startup in the background.",
                        EventLogEntryType.Warning);
                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("OpenCredential", e.ToString(), EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStop()
        {
            WaitForServiceInit();
            m_serviceThreadObj.Stop();
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
            WaitForServiceInit();
            m_serviceThreadObj.SessionChange(changeDescription);
        }

        private void WaitForServiceInit()
        {
            lock (this)
            {
                // If we are still initializing, wait
                if (m_serviceThread.IsAlive)
                    m_serviceThread.Join();
            }
        }
    }
}
