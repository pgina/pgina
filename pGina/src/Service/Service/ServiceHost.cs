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
using System.Runtime.InteropServices;

using pGina.Service.Impl;

using log4net;

namespace Service
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SERVICE_STATUS
    {
        public int serviceType;
        public int currentState;
        public int controlsAccepted;
        public int win32ExitCode;
        public int serviceSpecificExitCode;
        public int checkPoint;
        public int waitHint;
    }
    public enum State
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }
    public enum ServiceAccept
    {
        SERVICE_ACCEPT_NETBINDCHANGE = 0x00000010,
        SERVICE_ACCEPT_PARAMCHANGE = 0x00000008,
        SERVICE_ACCEPT_PAUSE_CONTINUE = 0x00000002,
        SERVICE_ACCEPT_PRESHUTDOWN = 0x00000100,
        SERVICE_ACCEPT_SHUTDOWN = 0x00000004,
        SERVICE_ACCEPT_STOP = 0x00000001,
        SERVICE_ACCEPT_HARDWAREPROFILECHANGE = 0x00000020,
        SERVICE_ACCEPT_POWEREVENT = 0x00000040,
        SERVICE_ACCEPT_SESSIONCHANGE = 0x00000080,
        SERVICE_ACCEPT_TIMECHANGE = 0x00000200,
        SERVICE_ACCEPT_TRIGGEREVENT = 0x00000400,
        SERVICE_ACCEPT_USERMODEREBOOT = 0x00000800, //windows 8
    }
    public enum ServiceControl
    {
        SERVICE_CONTROL_DEVICEEVENT = 0x0000000B,
        SERVICE_CONTROL_HARDWAREPROFILECHANGE = 0x0000000C,
        SERVICE_CONTROL_POWEREVENT = 0x0000000D,
        SERVICE_CONTROL_SESSIONCHANGE = 0x0000000E,
        SERVICE_CONTROL_TIMECHANGE = 0x00000010,
        SERVICE_CONTROL_TRIGGEREVENT = 0x00000020,
        SERVICE_CONTROL_USERMODEREBOOT = 0x00000040,
        SERVICE_CONTROL_CONTINUE = 0x00000003,
        SERVICE_CONTROL_INTERROGATE = 0x00000004,
        SERVICE_CONTROL_NETBINDADD = 0x00000007,
        SERVICE_CONTROL_NETBINDDISABLE = 0x0000000A,
        SERVICE_CONTROL_NETBINDENABLE = 0x00000009,
        SERVICE_CONTROL_NETBINDREMOVE = 0x00000008,
        SERVICE_CONTROL_PARAMCHANGE = 0x00000006,
        SERVICE_CONTROL_PAUSE = 0x00000002,
        SERVICE_CONTROL_PRESHUTDOWN = 0x0000000F,
        SERVICE_CONTROL_SHUTDOWN = 0x00000005,
        SERVICE_CONTROL_STOP = 0x00000001,
    }
    public enum INFO_LEVEL : uint
    {
        SERVICE_CONFIG_DESCRIPTION = 0x00000001,
        SERVICE_CONFIG_FAILURE_ACTIONS = 0x00000002,
        SERVICE_CONFIG_DELAYED_AUTO_START_INFO = 0x00000003,
        SERVICE_CONFIG_FAILURE_ACTIONS_FLAG = 0x00000004,
        SERVICE_CONFIG_SERVICE_SID_INFO = 0x00000005,
        SERVICE_CONFIG_REQUIRED_PRIVILEGES_INFO = 0x00000006,
        SERVICE_CONFIG_PRESHUTDOWN_INFO = 0x00000007,
        SERVICE_CONFIG_TRIGGER_INFO = 0x00000008,
        SERVICE_CONFIG_PREFERRED_NODE = 0x00000009
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct SERVICE_PRESHUTDOWN_INFO
    {
        public UInt32 dwPreshutdownTimeout;
    }
    public partial class pGinaServiceHost : ServiceBase
    {
        private Thread m_serviceThread = null;
        private pGina.Service.Impl.ServiceThread m_serviceThreadObj = null;
        const int SERVICE_ACCEPT_PRESHUTDOWN = 0x100;
        const int SERVICE_CONTROL_PRESHUTDOWN = 0xf;

        private ILog m_logger = LogManager.GetLogger("Pgina Service");

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool SetServiceStatus(IntPtr hServiceStatus, ref SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeServiceConfig2(IntPtr hService, int dwInfoLevel, IntPtr lpInfo);

        private SERVICE_STATUS myServiceStatus;

        public pGinaServiceHost()
        {
            InitializeComponent();

            FieldInfo fi = typeof(ServiceBase).GetField("acceptedCommands", BindingFlags.Instance | BindingFlags.NonPublic);
            int val = (int)fi.GetValue(this) | (int)ServiceAccept.SERVICE_ACCEPT_PRESHUTDOWN;
            fi.SetValue(this, val);

            //prepare myServiceStatus for further use
            using (ServiceController sc = new ServiceController(this.ServiceName))
            {
                myServiceStatus.serviceType = (int)sc.ServiceType;
                myServiceStatus.controlsAccepted = val;
                myServiceStatus.serviceSpecificExitCode = 0;
                myServiceStatus.win32ExitCode = 0;

                SERVICE_PRESHUTDOWN_INFO spi = new SERVICE_PRESHUTDOWN_INFO();
                spi.dwPreshutdownTimeout = 3600 * 1000;
                IntPtr lpInfo = Marshal.AllocHGlobal(Marshal.SizeOf(spi));
                IntPtr sHandle = sc.ServiceHandle.DangerousGetHandle();
                if (lpInfo == IntPtr.Zero)
                {
                    string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                    m_logger.ErrorFormat("Unable to allocate memory for service action, error: {0}", errorMessage);
                    EventLog.WriteEntry("pGina", String.Format("Unable to allocate memory for service action\nerror:{0}", errorMessage), EventLogEntryType.Warning);
                }
                else
                {
                    Marshal.StructureToPtr(spi, lpInfo, false);
                    if (!ChangeServiceConfig2(sHandle, (int)INFO_LEVEL.SERVICE_CONFIG_PRESHUTDOWN_INFO, lpInfo))
                    {
                        string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                        m_logger.ErrorFormat("ChangeServiceConfig2 error: {0}", errorMessage);
                        EventLog.WriteEntry("pGina", String.Format("ChangeServiceConfig2\nThe service will be forced to stop during a shutdown within 3 minutes\nerror:{0}", errorMessage), EventLogEntryType.Warning);
                    }
                    Marshal.FreeHGlobal(lpInfo);
                }
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
                case (int)ServiceControl.SERVICE_CONTROL_PRESHUTDOWN:
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
            while (m_serviceThreadObj.OnCustomCommand())
            {
                myServiceStatus.checkPoint++;
                myServiceStatus.currentState = (int)State.SERVICE_STOP_PENDING;
                myServiceStatus.waitHint = new TimeSpan(0,3,0).Milliseconds;
                if (!SetServiceStatus(this.ServiceHandle, ref myServiceStatus))
                {
                    string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                    m_logger.ErrorFormat("RequestAdditionalTime failed with {0}", errorMessage);
                }
                else
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
