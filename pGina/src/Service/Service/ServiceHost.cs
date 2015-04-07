/*
	Copyright (c) 2014, pGina Team
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
using System.Reflection;

using pGina.Service.Impl;

using log4net;

namespace Service
{
    public partial class pGinaServiceHost : ServiceBase
    {
        private Thread m_serviceThread = null;
        private pGina.Service.Impl.ServiceThread m_serviceThreadObj = null;
        private ILog m_logger = LogManager.GetLogger("Pgina Service");
        private Abstractions.WindowsApi.pInvokes.structenums.SERVICE_STATUS myServiceStatus;

        public pGinaServiceHost()
        {
            InitializeComponent();

            FieldInfo fi = typeof(ServiceBase).GetField("acceptedCommands", BindingFlags.Instance | BindingFlags.NonPublic);
            int val = (int)fi.GetValue(this) | (int)Abstractions.WindowsApi.pInvokes.structenums.ServiceAccept.SERVICE_ACCEPT_PRESHUTDOWN;
            fi.SetValue(this, val);

            using (ServiceController sc = new ServiceController(this.ServiceName))
            {
                myServiceStatus.serviceType = (int)sc.ServiceType;
                myServiceStatus.controlsAccepted = val;
                myServiceStatus.serviceSpecificExitCode = 0;
                myServiceStatus.win32ExitCode = 0;

                Abstractions.WindowsApi.pInvokes.SetPreshutdownTimeout(sc.ServiceHandle, ref myServiceStatus);
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                m_serviceThreadObj = new pGina.Service.Impl.ServiceThread();
                m_serviceThread = new Thread(new ThreadStart(m_serviceThreadObj.Start));
                m_serviceThread.Start();
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("pGina", e.ToString(), EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStop()
        {
            WaitForServiceInit();
            m_serviceThreadObj.Stop();
        }

        protected override void OnCustomCommand(int command)
        {
            WaitForServiceInit();
            switch (command)
            {
                case (int)Abstractions.WindowsApi.pInvokes.structenums.ServiceControl.SERVICE_CONTROL_PRESHUTDOWN:
                    Thread postpone = new Thread(SignalShutdownPending);
                    postpone.Start();
                    break;
                default:
                    base.OnCustomCommand(command);
                    break;
            }
        }
        private void SignalShutdownPending()
        {
            m_logger.Info("Preshutdown Event received");
            //DateTime end = DateTime.Now.AddMinutes(5);
            //while (end.Ticks > DateTime.Now.Ticks) //delay shutdown to n minutes, testing only
            while (m_serviceThreadObj.OnCustomCommand())
            {
                if (Abstractions.WindowsApi.pInvokes.ShutdownPending(this.ServiceHandle, ref myServiceStatus, new TimeSpan(0, 3, 0)))
                {
                    //m_logger.Info("RequestAdditionalTime suceeded");
                }
                Thread.Sleep(1000);
            }
            m_logger.Info("RequestAdditionalTime finished");
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
