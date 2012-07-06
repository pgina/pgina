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
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.Configuration.Install;
using System.Reflection;

using log4net;

namespace Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (System.Environment.UserInteractive)
            {
                // Initalize logging and grab a logger
                pGina.Shared.Logging.Logging.Init();
                ILog m_log = LogManager.GetLogger("Service Install");

                try
                {
                    string parameter = string.Concat(args);
                    switch (parameter)
                    {
                        case "--install":
                            m_log.DebugFormat("Installing service...");
                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                            break;
                        case "--uninstall":
                            m_log.DebugFormat("Uninstalling service...");
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                            break;
                        case "--start":
                            m_log.Debug("Starting service...");
                            Start(args);
                            break;
                        case "--stop":
                            m_log.Debug("Stopping service...");
                            Stop();
                            break;
                    }
                }
                catch (Exception e)
                {
                    m_log.ErrorFormat("Uncaught exception in UserInteractive mode: {0}", e);
                    Environment.Exit(1);
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
				    new pGinaServiceHost() 
			    };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static void Start(string[] args)
        {
            foreach (ServiceController ctrl in ServiceController.GetServices())
            {
                if (ctrl.ServiceName == "pGina")
                {
                    ctrl.Start(args);
                    break;
                }
            }
        }

        private static void Stop()
        {
            foreach (ServiceController ctrl in ServiceController.GetServices())
            {
                if (ctrl.ServiceName == "pGina")
                {
                    ctrl.Stop();
                    break;
                }
            }
        }
    }
}
