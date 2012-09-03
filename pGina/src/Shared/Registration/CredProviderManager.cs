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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Win32;

using log4net;
using pGina.Shared.Settings;

namespace pGina.CredentialProvider.Registration
{

    public abstract class CredProviderManager
    {
        public Settings CpInfo { get; set; }

        public CredProviderManager()
        {
            CpInfo = new Settings();
        }

        public static CredProviderManager GetManager()
        {
            if (Abstractions.Windows.OsInfo.IsWindows())
            {
                if (Abstractions.Windows.OsInfo.IsVistaOrLater())
                    return new DefaultCredProviderManager();
                else
                    return new GinaCredProviderManager();
            }
            else
            {
                throw new Exception("Must be executed on a Windows OS.");
            }
        }

        public void ExecuteDefaultAction()
        {
            switch (CpInfo.OpMode)
            {
                case OperationMode.INSTALL:
                    this.Install();
                    break;
                case OperationMode.UNINSTALL:
                    this.Uninstall();
                    break;
                case OperationMode.DISABLE:
                    this.Disable();
                    break;
                case OperationMode.ENABLE:
                    this.Enable();
                    break;
            }
        }

        public abstract void Install();
        public abstract void Uninstall();
        public abstract void Disable();
        public abstract void Enable();

        public abstract bool Registered();
        public abstract bool Registered6432();
        public abstract bool Enabled();
        public abstract bool Enabled6432();
    }

    public class GinaCredProviderManager : CredProviderManager
    {

        private static readonly string GINA_KEY = @"Software\Microsoft\Windows NT\CurrentVersion\Winlogon";
        private ILog m_logger = LogManager.GetLogger("GinaCredProviderManager");

        public GinaCredProviderManager()
        {
            this.CpInfo.ShortName = "pGinaGINA";
        }

        public override void Install()
        {
            FileInfo dll = null;

            if (Abstractions.Windows.OsInfo.Is64Bit())
            {
                dll = DllUtils.Find64BitDll(this.CpInfo.Path, this.CpInfo.ShortName);
            }
            else
            {
                dll = DllUtils.Find32BitDll(this.CpInfo.Path, this.CpInfo.ShortName);
            }
            if (dll != null)
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(GINA_KEY, true))
                {
                    if (key != null)
                    {
                        m_logger.DebugFormat("{0} {1} => {2}", key.ToString(), "GinaDLL",
                            dll.FullName);
                        key.SetValue("GinaDLL", dll.FullName);
                        key.SetValue("NoDomainUI", 1);
                        key.SetValue("DontDisplayLastUserName", 1);
                    }
                }
            }
            else
            {
                throw new Exception("GINA DLL not found in " + CpInfo.Path);
            }
        }

        public override void Uninstall()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(GINA_KEY, true))
            {
                if (key != null)
                {
                    m_logger.DebugFormat("Deleting GinaDLL value in {0}", key.ToString());
                    key.DeleteValue("GinaDLL", false);
                    key.DeleteValue("NoDomainUI", false);
                }
            }
        }

        public override void Disable()
        {
            dynamic pGinaSettings = new pGinaDynamicSettings();
            pGinaSettings.GinaPassthru = true;
        }

        public override void Enable()
        {
            dynamic pGinaSettings = new pGinaDynamicSettings();
            pGinaSettings.GinaPassthru = false;
        }

        public override bool Registered()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(GINA_KEY))
            {
                if (key != null)
                {
                    object value = key.GetValue("GinaDLL");
                    return value != null;
                }
                else
                {
                    return false;
                }
            }
        }

        public override bool Registered6432()
        {
            return true;
        }

        public override bool Enabled()
        {
            dynamic pGinaSettings = new pGinaDynamicSettings();
            bool passthru = pGinaSettings.GetSetting("GinaPassthru", false);
            return !passthru;
        }

        public override bool Enabled6432()
        {
            return true;
        }        
    }

    public class DefaultCredProviderManager : CredProviderManager
    {
        /*
         [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers\{**GUID**}]
         @="pGinaCredentialProvider"

         [HKEY_CLASSES_ROOT\CLSID\{**GUID**}]
         @="pGinaCredentialProvider"

         [HKEY_CLASSES_ROOT\CLSID\{**GUID**}\InprocServer32]
         @="SampleCredUICredentialProvider.dll"
         "ThreadingModel"="Apartment"
        */

        private ILog m_logger = LogManager.GetLogger("DefaultCredProviderManager");

        static readonly string PROVIDER_KEY_BASE = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers";
        static readonly string CP_FILTER_KEY_BASE = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\Credential Provider Filters";
        static readonly string CLSID_BASE = @"CLSID";
        static readonly string PROVIDER_KEY_BASE_6432 = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Authentication\Credential Providers";
        static readonly string CP_FILTER_KEY_BASE_6432 = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Authentication\Credential Provider Filters";
        static readonly string WINDIR = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        // The registry keys
        string ProviderKey 
        { 
            get { return string.Format(@"{0}\{{{1}}}", PROVIDER_KEY_BASE, CpInfo.ProviderGuid.ToString()); }
        }
        string CredentialProviderFilterKey
        {
            get { return string.Format(@"{0}\{{{1}}}", CP_FILTER_KEY_BASE, CpInfo.ProviderGuid.ToString()); }
        }
        string ClsidRoot
        {
            get { return string.Format(@"{0}\{{{1}}}", CLSID_BASE, CpInfo.ProviderGuid.ToString()); }
        }
        string ClsidInProc
        {
            get { return string.Format(@"{0}\InprocServer32", this.ClsidRoot); }
        }
        string ProviderKey6432
        {
            get { return string.Format(@"{0}\{{{1}}}", PROVIDER_KEY_BASE_6432, CpInfo.ProviderGuid.ToString()); }
        }
        string CredentialProviderFilterKey6432
        {
            get { return string.Format(@"{0}\{{{1}}}", CP_FILTER_KEY_BASE_6432, CpInfo.ProviderGuid.ToString()); }
        }

        public DefaultCredProviderManager()
        {
            // Defaults for pGina Credential Provider
            this.CpInfo.ProviderGuid = new Guid("{D0BEFEFB-3D2C-44DA-BBAD-3B2D04557246}");
            this.CpInfo.ShortName = "pGinaCredentialProvider";
        }

        private string GetCpDllPath()
        {
            return Path.Combine(WINDIR, "System32", CpInfo.ShortName + ".dll");
        }

        private string GetCpDllPath6432()
        {
            return Path.Combine(WINDIR, "Syswow64", CpInfo.ShortName + ".dll");
        }

        public override void Install()
        {
            m_logger.InfoFormat("Installing credential provider {0} {{{1}}}",
                CpInfo.ShortName,
                CpInfo.ProviderGuid.ToString());

            // Copy the DLL
            if (Abstractions.Windows.OsInfo.Is64Bit()) // Are we on a 64 bit OS?
            {
                FileInfo x64Dll = DllUtils.Find64BitDll(CpInfo.Path, CpInfo.ShortName);
                FileInfo x32Dll = DllUtils.Find32BitDll(CpInfo.Path, CpInfo.ShortName);
                string destination64 = GetCpDllPath();
                string destination32 = GetCpDllPath6432();

                if (x64Dll == null && x32Dll == null)
                {
                    throw new Exception("No 64 or 32 bit DLL found in: " + CpInfo.Path);
                }

                if (x64Dll != null)
                {
                    m_logger.DebugFormat("Found 64 bit DLL: {0}", x64Dll.FullName);
                    m_logger.DebugFormat("   copying to: {0}", destination64);
                    File.Copy(x64Dll.FullName, destination64, true);
                }
                else
                {
                    m_logger.Error("WARNING: No 64 bit DLL found.");
                }

                if (x32Dll != null)
                {
                    m_logger.DebugFormat("Found 32 bit DLL: {0}", x32Dll.FullName);
                    m_logger.DebugFormat("   copying to: {0}", destination32);

                    File.Copy(x32Dll.FullName, destination32, true);

                    // Write registry keys for 32 bit DLL
                    // The provider
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(this.ProviderKey6432))
                    {
                        key.SetValue("", CpInfo.ShortName);
                    }
                    // The provider filter
                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(this.CredentialProviderFilterKey6432))
                    {
                        key.SetValue("", CpInfo.ShortName);
                    }
                }
                else
                {
                    m_logger.Error("WARNING: No 32 bit DLL found.");
                }
            }
            else
            {
                FileInfo x32Dll = DllUtils.Find32BitDll(CpInfo.Path, CpInfo.ShortName);
                string destination = GetCpDllPath();

                if (x32Dll != null)
                {
                    m_logger.DebugFormat("Found 32 bit DLL: {0}", x32Dll.FullName);
                    m_logger.DebugFormat("   copying to: {0}", destination);

                    File.Copy(x32Dll.FullName, destination, true);
                }
                else
                {
                    throw new Exception("No 32 bit DLL found in: " + CpInfo.Path);
                }
            }

            m_logger.Debug("Writing registry entries...");

            // Write registry values
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(this.ProviderKey))
            {
                m_logger.DebugFormat("{0} @=> {1}", key.ToString(), CpInfo.ShortName);
                key.SetValue("", CpInfo.ShortName);
            }
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(this.CredentialProviderFilterKey))
            {
                m_logger.DebugFormat("{0} @=> {1}", key.ToString(), CpInfo.ShortName);
                key.SetValue("", CpInfo.ShortName);
            }
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(this.ClsidRoot))
            {
                m_logger.DebugFormat("{0} @=> {1}", key.ToString(), CpInfo.ShortName);
                key.SetValue("", CpInfo.ShortName);
            }
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(this.ClsidInProc))
            {
                m_logger.DebugFormat("{0} @=> {1}", key.ToString(), CpInfo.ShortName);
                key.SetValue("", CpInfo.ShortName);
                m_logger.DebugFormat("{0} {1} => {2}", key.ToString(), "ThreadingModel", "Apartment");
                key.SetValue("ThreadingModel", "Apartment");
            }
        }

        public override void Uninstall()
        {
            m_logger.InfoFormat("Uninstalling credential provider {0} {{{1}}}",
                CpInfo.ShortName,
                CpInfo.ProviderGuid.ToString());

            string dll = GetCpDllPath();
            string dll6432 = GetCpDllPath6432();
            
            if (File.Exists(dll))
            {
                m_logger.DebugFormat("Deleting: {0}", dll);
                File.Delete(dll);
            }
            if (File.Exists(dll6432))
            {
                m_logger.DebugFormat("Deleting: {0}", dll6432);
                File.Delete(dll6432);
            }

            string guid = "{" + CpInfo.ProviderGuid.ToString() + "}";
            DeleteRegistryKey(Registry.LocalMachine, PROVIDER_KEY_BASE, guid);
            DeleteRegistryKey(Registry.LocalMachine, CP_FILTER_KEY_BASE, guid);
            DeleteRegistryKey(Registry.LocalMachine, PROVIDER_KEY_BASE_6432, guid);
            DeleteRegistryKey(Registry.LocalMachine, CP_FILTER_KEY_BASE_6432, guid);
            DeleteRegistryKey(Registry.ClassesRoot, CLSID_BASE, string.Format("{0}\\{1}", guid, "InprocServer32"));
            DeleteRegistryKey(Registry.ClassesRoot, CLSID_BASE, guid);
        }

        private void DeleteRegistryKey(RegistryKey baseKey, string parentSubKey, string childSubKey)
        {
            using (RegistryKey key = baseKey.OpenSubKey(parentSubKey, true))
            {
                if (key != null)
                {
                    m_logger.DebugFormat("Deleting {0}\\{1}", key.ToString(), childSubKey);
                    key.DeleteSubKey(childSubKey,false);
                }
            }
        }

        public override void Disable()
        {
            m_logger.InfoFormat("Disabling credential provider: {0} {{{1}}}",
                CpInfo.ShortName,
                CpInfo.ProviderGuid.ToString());

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(this.ProviderKey, true))
            {
                if (key != null)
                {
                    m_logger.DebugFormat("Writing {0}: {1} => {2}", key.ToString(), "Disabled", 1);
                    key.SetValue("Disabled", 1);
                }
                else
                {
                    m_logger.Error("WARNING: No credential provider registry entry found for that GUID.");
                }
            }

            if (Abstractions.Windows.OsInfo.Is64Bit())
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(this.ProviderKey6432, true))
                {
                    if (key != null)
                    {
                        m_logger.DebugFormat("Writing {0}: {1} => {2}", key.ToString(), "Disabled", 1);
                        key.SetValue("Disabled", 1);
                    }
                    else
                    {
                        m_logger.Error("WARNING: No 32-bit registry entry found with that GUID.");
                    }
                }
            }
        }

        public override void Enable()
        {
            m_logger.InfoFormat("Enabling credential provider: {0} {{{1}}}",
                CpInfo.ShortName,
                CpInfo.ProviderGuid.ToString());

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(this.ProviderKey, true))
            {
                if (key != null)
                {
                    m_logger.DebugFormat("Deleting {0}: {1}", key.ToString(), "Disabled");

                    if (key.GetValue("Disabled") != null)
                        key.DeleteValue("Disabled");
                }
                else
                {
                    m_logger.Error("WARNING: Did not find a registry entry for that GUID.");
                }
            }

            if (Abstractions.Windows.OsInfo.Is64Bit())
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(this.ProviderKey6432, true))
                {
                    if (key != null)
                    {
                        m_logger.DebugFormat("Deleting {0}: {1}", key.ToString(), "Disabled");

                        if (key.GetValue("Disabled") != null)
                            key.DeleteValue("Disabled");
                    }
                    else
                    {
                        m_logger.Error("WARNING: Did not find a (32 bit) registry entry for that GUID.");
                    }
                }
            }
        }

        public override bool Registered()
        {
            bool result = false;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(this.ProviderKey))
            {
                if (key != null)
                    result = true;
            }
            return result;
        }

        public override bool Enabled()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(this.ProviderKey))
            {
                if (key == null) return false;
                object value = key.GetValue("Disabled");
                if (value == null) return true;
                else
                {
                    return (int)value == 0;
                }
            }
        }

        public override bool Registered6432()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(this.ProviderKey6432))
            {
                return key != null;
            }
        }

        public override bool Enabled6432()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(this.ProviderKey6432))
            {
                if (key == null) return false;
                object value = key.GetValue("Disabled");
                if (value == null) return true;
                else
                {
                    return (int)value == 0;
                }
            }
        }
    }
}
