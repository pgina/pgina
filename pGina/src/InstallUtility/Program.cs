/*
	Copyright (c) 2012, pGina Team
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
using System.ServiceProcess;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Reflection;
using Microsoft.Win32;

using log4net;
using System.Configuration.Install;

namespace pGina.InstallUtil
{
    class Program
    {
        static readonly string PGINA_SERVICE_NAME = "pGina";
        static readonly string PGINA_SERVICE_EXE = "pGina.Service.ServiceHost.exe";
        
        // Initalized in the static constructor
        static readonly SecurityIdentifier ADMIN_GROUP;
        static readonly SecurityIdentifier USERS_GROUP;
        static readonly SecurityIdentifier SYSTEM_ACCT;
        static readonly SecurityIdentifier AUTHED_USERS;
        private static readonly string INSTALL_UTIL_PATH;
        private static readonly string PGINA_SERVICE_FULL_PATH;
        
        static Program()
        {
            // Init logging
            pGina.Shared.Logging.Logging.Init();
            
            // Intialize readonly variables

            PGINA_SERVICE_FULL_PATH = Path.Combine(
               Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), PGINA_SERVICE_EXE);
            INSTALL_UTIL_PATH = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(),
                "installutil.exe");

            ADMIN_GROUP = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            USERS_GROUP = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            SYSTEM_ACCT = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            AUTHED_USERS = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
        }

        static ILog m_logger = LogManager.GetLogger("pGina.InstallUtil");

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                m_logger.Error("Missing argument.  Must be one of \"post-install\" or \"post-uninstall\".");
                return 1;
            }

            try
            {
                if (args[0].Equals("post-install", StringComparison.CurrentCultureIgnoreCase))
                {
                    DoPostInstall();
                }
                else if (args[0].Equals("post-uninstall", StringComparison.CurrentCultureIgnoreCase))
                {
                    DoPostUninstall();
                }
                else
                {
                    m_logger.ErrorFormat("Unrecognzied action: {0}", args[0]);
                    return 1;
                }
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Exception occured: {0}", e);
                return 1;
            }

            return 0;
        }

        private static void DoPostInstall()
        {
            SetRegistryAcls();
            InstallAndStartService();
            RegisterAndEnableCredentialProvider();
            UpdatePluginPath();
        }

        /// <summary>
        /// If the current plugin path is the default for pGina 3.1.6 and earlier, 
        /// update it for later versions.
        /// </summary>
        private static void UpdatePluginPath()
        {
            m_logger.Debug("Checking plugin path to see if it needs to be updated.");
            string pluginsBaseDir = 
                string.Format( @"{0}\Plugins",
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            dynamic settings = new pGina.Shared.Settings.pGinaDynamicSettings();
            string[] pluginDirs = settings.PluginDirectories;
            if (pluginDirs.Length == 1 &&
                pluginDirs[0].Equals(pluginsBaseDir, StringComparison.CurrentCultureIgnoreCase))
            {
                m_logger.Info("Updating plugin path for core/contrib subdirectories.");
                settings.PluginDirectories = new string[] { 
                    string.Format(@"{0}\Core", pluginsBaseDir),
                    string.Format(@"{0}\Contrib", pluginsBaseDir)
                };
            }
        }

        private static void DoPostUninstall()
        {
            // Uninstall service
            if (ServiceInstalled())
            {
                StopService();
                UninstallService();
            }
            else
            {
                m_logger.Info("pGina service is not installed, skipping uninstall.");
            }

            // Uninstall CP
            UninstallCredentialProvider();
        }

        private static void SetRegistryAcls()
        {
            string pGinaSubKey = pGina.Shared.Settings.pGinaDynamicSettings.pGinaRoot;

            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(pGinaSubKey))
            {
                if (key != null)
                {
                    m_logger.InfoFormat("Setting ACLs on {0}", key.Name);

                    RegistryAccessRule allowRead = new RegistryAccessRule(
                        USERS_GROUP, RegistryRights.ReadKey, 
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None, AccessControlType.Allow);
                    RegistryAccessRule adminFull = new RegistryAccessRule(
                        ADMIN_GROUP, RegistryRights.FullControl,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None, AccessControlType.Allow);
                    RegistryAccessRule systemFull = new RegistryAccessRule(
                        SYSTEM_ACCT, RegistryRights.FullControl,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None, AccessControlType.Allow);
                    
                    RegistrySecurity keySec = key.GetAccessControl();

                    if (m_logger.IsDebugEnabled)
                    {
                        m_logger.DebugFormat("{0} before update:", key.Name);
                        ShowSecurity(keySec);
                    }

                    // Remove inherited rules
                    keySec.SetAccessRuleProtection(true, false);

                    // Add full control for administrators and system.
                    keySec.AddAccessRule(adminFull);
                    keySec.AddAccessRule(systemFull);

                    // Remove any read rules for users (if they exist)
                    keySec.RemoveAccessRuleAll(allowRead);

                    // Apply the rules..
                    key.SetAccessControl(keySec);

                    if (m_logger.IsDebugEnabled)
                    {
                        m_logger.DebugFormat("{0} after update: ", key.Name);
                        ShowSecurity(keySec);
                    }
                }
            }
        }

        private static void RegisterAndEnableCredentialProvider()
        {
            pGina.CredentialProvider.Registration.CredProviderManager cpManager =
                pGina.CredentialProvider.Registration.CredProviderManager.GetManager();

            m_logger.Info("Registering CP/GINA....");
            cpManager.CpInfo.OpMode = CredentialProvider.Registration.OperationMode.INSTALL;
            cpManager.ExecuteDefaultAction();

            m_logger.Info("Enabling CP/GINA...");
            cpManager.CpInfo.OpMode = CredentialProvider.Registration.OperationMode.ENABLE;
            cpManager.ExecuteDefaultAction();
        }

        private static void UninstallCredentialProvider()
        {
            pGina.CredentialProvider.Registration.CredProviderManager cpManager =
                pGina.CredentialProvider.Registration.CredProviderManager.GetManager();

            m_logger.Info("Uninstalling CP/GINA....");
            cpManager.CpInfo.OpMode = CredentialProvider.Registration.OperationMode.UNINSTALL;
            cpManager.ExecuteDefaultAction();
        }

        private static void InstallAndStartService()
        {
            if (!File.Exists(PGINA_SERVICE_EXE))
            {
                throw new Exception("The service executable was not found.");
            }

            if (ServiceInstalled())
            {
                m_logger.Warn("Service already installed, re-installing.");
                StopService();
                UninstallService();
            }

            InstallService();
            StartService();
        }

        private static bool ServiceInstalled()
        {
            using (ServiceController pGinaService = GetServiceController())
            {
                return pGinaService != null;
            }
        }

        private static void StopService()
        {
            using (ServiceController pGinaService = GetServiceController())
            {
                if (pGinaService != null)
                {
                    if (pGinaService.Status == ServiceControllerStatus.Running)
                    {
                        m_logger.InfoFormat("Stopping pGina service...");
                        pGinaService.Stop();
                    }
                }
                else
                    throw new Exception("pGina service not installed");
            }
        }

        private static void StartService()
        {
            using (ServiceController pGinaService = GetServiceController())
            {
                if (pGinaService != null)
                {
                    if ( pGinaService.Status != ServiceControllerStatus.Running )
                    {
                        m_logger.InfoFormat("Starting pGina service...");
                        pGinaService.Start();
                    }
                }
                else
                    throw new Exception("pGina service not installed");
            }
        }

        private static void UninstallService()
        {
            m_logger.InfoFormat("Uninstalling pGina service...");

            // If we can find the .NET installutil.exe, run that, otherwise, use 
            // ManagedInstallerClass (not recommended by MSDN, but works).
            if (File.Exists(INSTALL_UTIL_PATH))
            {
                // Need quotes around the path when calling installutil.exe
                string[] args = { "/u", string.Format("\"{0}\"", PGINA_SERVICE_FULL_PATH) };
                // Call the .NET installutil.exe
                CallInstallUtil(args);   
            }
            else
            {
                m_logger.DebugFormat("Can't find .NET installutil.exe ({0}), trying ManagedInstallerClass.InstallHelper", INSTALL_UTIL_PATH);
                ManagedInstallerClass.InstallHelper(new string[] { "/u", PGINA_SERVICE_FULL_PATH });
            }
        }

        private static void InstallService()
        {
            m_logger.InfoFormat("Installing pGina service...");

            // If we can find the .NET installutil.exe, run that, otherwise, use 
            // ManagedInstallerClass (not recommended by MSDN, but works).
            if (File.Exists(INSTALL_UTIL_PATH))
            {
                // Need quotes around the path when calling installutil.exe
                string[] args = { string.Format( "\"{0}\"", PGINA_SERVICE_FULL_PATH ) };
                // Call the .NET installutil.exe
                CallInstallUtil(args);
            }
            else
            {
                m_logger.DebugFormat("Can't find .NET installutil.exe ({0}), trying ManagedInstallerClass.InstallHelper", INSTALL_UTIL_PATH);
                ManagedInstallerClass.InstallHelper(new string[] {PGINA_SERVICE_FULL_PATH});
            }
        }

        private static ServiceController GetServiceController()
        {
            ServiceController pGinaService = null;

            foreach (ServiceController ctrl in ServiceController.GetServices())
            {
                if (ctrl.ServiceName == PGINA_SERVICE_NAME)
                {
                    pGinaService = ctrl;
                    break;
                }
            }

            return pGinaService;
        }

        private static void ShowSecurity(RegistrySecurity security)
        {
            foreach (RegistryAccessRule ar in security.GetAccessRules(true, true, typeof(NTAccount)))
            {
                m_logger.DebugFormat("           User: {0}", ar.IdentityReference);
                m_logger.DebugFormat("           Type: {0}", ar.AccessControlType);
                m_logger.DebugFormat("         Rights: {0}", ar.RegistryRights);
                m_logger.DebugFormat("    Inheritance: {0}", ar.InheritanceFlags);
                m_logger.DebugFormat("    Propagation: {0}", ar.PropagationFlags);
                m_logger.DebugFormat("      Inherited? {0}", ar.IsInherited);
            }
        }

        private static void CallInstallUtil(string[] args)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = INSTALL_UTIL_PATH;
            proc.StartInfo.Arguments = String.Join(" ", args);
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.ErrorDataReceived += new DataReceivedEventHandler(proc_ErrorDataReceived);
            proc.OutputDataReceived += new DataReceivedEventHandler(proc_OutputDataReceived);
            proc.StartInfo.UseShellExecute = false;

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            //  ---check result---
            if (proc.ExitCode != 0)
            {
                throw new Exception(String.Format("InstallUtil error -- code {0}", proc.ExitCode));
            }
        }

        static void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            m_logger.Info(e.Data);
        }

        static void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            m_logger.Error(e.Data);
        }
    }
}
