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
using System.ServiceProcess;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;

using log4net;

namespace pGina.InstallUtil
{
    class Program
    {
        static readonly string PGINA_SERVICE_NAME = "pGina";
        static readonly string PGINA_SERVICE_EXE = "pGina.Service.ServiceHost.exe";
        static readonly string PGINA_CP_REGISTRATION_EXE = "pGina.CredentialProvider.Registration.exe";
        static readonly string PGINA_CONFIG_EXE = "pGina.Configuration.exe";
        static readonly SecurityIdentifier ADMIN_GROUP = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        static readonly SecurityIdentifier USERS_GROUP = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
        static readonly SecurityIdentifier SYSTEM_ACCT = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
        static readonly SecurityIdentifier AUTHED_USERS = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
                   
        static Program()
        {
            // Init logging
            pGina.Shared.Logging.Logging.Init();
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

            // Probably not necessary or useful...
            //SetFileSystemAcls();
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

        private static void SetFileSystemAcls()
        {
            if (!File.Exists(PGINA_CONFIG_EXE))
            {
                throw new Exception(string.Format("Unable to find configuration executable: {0}", PGINA_CONFIG_EXE));
            }

            m_logger.InfoFormat("Setting ACLs on {0}", PGINA_CONFIG_EXE);

            FileSystemAccessRule userReadAndExecute = new FileSystemAccessRule(USERS_GROUP, FileSystemRights.ReadAndExecute, AccessControlType.Allow);
            FileSystemAccessRule userRead = new FileSystemAccessRule(USERS_GROUP, FileSystemRights.Read, AccessControlType.Allow);
            FileSystemAccessRule adminFull = new FileSystemAccessRule(ADMIN_GROUP, FileSystemRights.FullControl, AccessControlType.Allow);
            FileSystemAccessRule systemFull = new FileSystemAccessRule(SYSTEM_ACCT, FileSystemRights.FullControl, AccessControlType.Allow);
            FileSystemAccessRule authedUsersMod = new FileSystemAccessRule(AUTHED_USERS, FileSystemRights.Modify, AccessControlType.Allow);
            FileSystemAccessRule usersMod = new FileSystemAccessRule(USERS_GROUP, FileSystemRights.Modify, AccessControlType.Allow);
            FileSecurity fs = File.GetAccessControl(PGINA_CONFIG_EXE);

            fs.SetAccessRuleProtection(true, false);

            fs.RemoveAccessRuleAll(authedUsersMod);
            fs.RemoveAccessRuleAll(usersMod);
            fs.AddAccessRule(userReadAndExecute);
            fs.AddAccessRule(adminFull);
            fs.AddAccessRule(systemFull);

            File.SetAccessControl(PGINA_CONFIG_EXE, fs);
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
            if (!File.Exists(PGINA_CP_REGISTRATION_EXE))
            {
                throw new Exception("The registration executable was not found.");
            }

            m_logger.Info("Registering CP/GINA....");
            Process p = Process.Start(PGINA_CP_REGISTRATION_EXE, "--mode install");
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                m_logger.ErrorFormat("Error registering CP/GINA.");
                throw new Exception("Error registering CP/GINA.");
            }

            m_logger.Info("Enabling CP/GINA...");
            p = Process.Start(PGINA_CP_REGISTRATION_EXE, "--mode enable");
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                m_logger.ErrorFormat("Error enabling CP/GINA.");
                throw new Exception("Error enabling CP/GINA.");
            }
        }

        private static void UninstallCredentialProvider()
        {
            if (!File.Exists(PGINA_CP_REGISTRATION_EXE))
            {
                throw new Exception("The registration executable was not found.");
            }

            m_logger.Info("Uninstalling CP/GINA....");
            Process p = Process.Start(PGINA_CP_REGISTRATION_EXE, "--mode uninstall");
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                m_logger.ErrorFormat("Error uninstalling CP/GINA.");
                throw new Exception("Error uninstalling CP/GINA.");
            }
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
            }
        }

        private static void StartService()
        {
            m_logger.InfoFormat("Starting pGina service...");
            Process p = new Process();
            p.StartInfo.FileName = PGINA_SERVICE_EXE;
            p.StartInfo.Arguments = "--start";
            p.Start();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                m_logger.ErrorFormat("Failed to start service (exit code {0}).", p.ExitCode);
                throw new Exception("Failed to start service.");
            }
        }

        private static void UninstallService()
        {
            m_logger.InfoFormat("Uninstalling pGina service...");
            Process p = new Process();
            p.StartInfo.FileName = PGINA_SERVICE_EXE;
            p.StartInfo.Arguments = "--uninstall";
            p.Start();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                m_logger.ErrorFormat("Failed to uninstall service (exit code {0}).", p.ExitCode);
                throw new Exception("Failed to uninstall pGina service.");
            }
        }

        private static void InstallService()
        {
            m_logger.InfoFormat("Installing pGina service...");
            Process p = new Process();
            p.StartInfo.FileName = PGINA_SERVICE_EXE;
            p.StartInfo.Arguments = "--install";
            p.Start();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                m_logger.ErrorFormat("Failed to install service (exit code {0}).", p.ExitCode);
                throw new Exception("Service installation failed.");
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
    }
}
