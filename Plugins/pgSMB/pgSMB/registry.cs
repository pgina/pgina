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
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;
using System.ComponentModel;


using log4net;

//based on http://stackoverflow.com/questions/7894909/load-registry-hive-from-c-sharp-fails
namespace pGina.Plugin.pgSMB
{
    public enum RegistryLocation
    {
        HKEY_CLASSES_ROOT,
        HKEY_CURRENT_USER,
        HKEY_LOCAL_MACHINE,
        HKEY_USERS,
        HKEY_CURRENT_CONFIG
    }

    class registry
    {
        private static ILog m_logger = LogManager.GetLogger("pgSMB[registry]");
        private static Boolean privSet = false;

        #region Structs/Enums

        private const Int32 ANYSIZE_ARRAY = 1;
        private const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
        private const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const UInt32 TOKEN_QUERY = 0x0008;
        private const string SE_RESTORE_NAME = "SeRestorePrivilege";
        private const string SE_BACKUP_NAME = "SeBackupPrivilege";
        private static IntPtr _myToken;
        private static TokPriv1Luid _tokenPrivileges = new TokPriv1Luid();
        private static TokPriv1Luid _tokenPrivileges2 = new TokPriv1Luid();
        private static LUID _restoreLuid;
        private static LUID _backupLuid;

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID pLuid;
            public UInt32 Attributes;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TokPriv1Luid
        {
            public int Count;
            public LUID Luid;
            public UInt32 Attr;
        }

        public enum baseKey : uint
        {
            HKEY_CLASSES_ROOT = 0x80000000,
            HKEY_CURRENT_USER = 0x80000001,
            HKEY_LOCAL_MACHINE = 0x80000002,
            HKEY_USERS = 0x80000003,
            HKEY_CURRENT_CONFIG = 0x80000005
        }

        public static RegistryKey GetRegistryLocation(RegistryLocation location)
        {
            switch (location)
            {
                case RegistryLocation.HKEY_CLASSES_ROOT:
                    return Registry.ClassesRoot;

                case RegistryLocation.HKEY_CURRENT_USER:
                    return Registry.CurrentUser;

                case RegistryLocation.HKEY_LOCAL_MACHINE:
                    return Registry.LocalMachine;

                case RegistryLocation.HKEY_USERS:
                    return Registry.Users;

                case RegistryLocation.HKEY_CURRENT_CONFIG:
                    return Registry.CurrentConfig;

                default:
                    return null;

            }
        }
        #endregion

        #region kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);
        #endregion

        #region advapi32.dll
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool AdjustTokenPrivileges(IntPtr htok, bool disableAllPrivileges, ref TokPriv1Luid newState, int len, IntPtr prev, IntPtr relen);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegLoadKey(UInt32 hKey, String lpSubKey, String lpFile);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int RegUnLoadKey(UInt32 hKey, string lpSubKey);
        #endregion


        public static Boolean SetPrivilege()
        {
            m_logger.Info("Set privilege to load regfiles");

            if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out _myToken))
            {
                m_logger.ErrorFormat("SetPrivilege() OpenProcess Error: {0}", Marshal.GetLastWin32Error());
                return false;
            }

            if (!LookupPrivilegeValue(null, SE_RESTORE_NAME, out _restoreLuid))
            {
                m_logger.ErrorFormat("SetPrivilege() LookupPrivilegeValue Error: {0}", Marshal.GetLastWin32Error());
                return false;
            }

            if (!LookupPrivilegeValue(null, SE_BACKUP_NAME, out _backupLuid))
            {
                m_logger.ErrorFormat("SetPrivilege() LookupPrivilegeValue Error: {0}", Marshal.GetLastWin32Error());
                return false;
            }

            _tokenPrivileges.Attr = SE_PRIVILEGE_ENABLED;
            _tokenPrivileges.Luid = _restoreLuid;
            _tokenPrivileges.Count = 1;

            _tokenPrivileges2.Attr = SE_PRIVILEGE_ENABLED;
            _tokenPrivileges2.Luid = _backupLuid;
            _tokenPrivileges2.Count = 1;

            if (!AdjustTokenPrivileges(_myToken, false, ref _tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero))
            {
                m_logger.ErrorFormat("SetPrivilege() AdjustTokenPrivileges Error: {0}", Marshal.GetLastWin32Error());
                return false;
            }

            if (!AdjustTokenPrivileges(_myToken, false, ref _tokenPrivileges2, 0, IntPtr.Zero, IntPtr.Zero))
            {
                m_logger.ErrorFormat("SetPrivilege() AdjustTokenPrivileges Error: {0}", Marshal.GetLastWin32Error());
                return false;
            }

            if (!CloseHandle(_myToken))
                m_logger.Warn("Can't close handle _myToken");

            privSet = true;
            return true;
        }

        public static Boolean RegistryLoad(string where, string name, string file)
        {
            m_logger.InfoFormat("Load registry {0}", file);

            if (!privSet)
            {
                if (!SetPrivilege())
                    return false;
            }

            int ret = RegLoadKey((uint)Enum.Parse(typeof(baseKey), where), name, file);
            if (ret != 0)
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                m_logger.ErrorFormat("Unable to load regfile {0} error:{1} {2}", file, ret, errorMessage);
                return false;
            }
            return true;
        }

        public static Boolean RegistryUnLoad(string where, string name)
        {
            m_logger.InfoFormat("Unload registry {0}", name);

            if (!privSet)
            {
                if (!SetPrivilege())
                    return false;
            }

            int ret = RegUnLoadKey((uint)Enum.Parse(typeof(baseKey), where), name);
            if (ret != 0)
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                m_logger.ErrorFormat("Unable to unload regkey {0} error:{1} {2}", name, ret, errorMessage);
                return false;
            }
            return true;
        }

        public static Boolean RegQuota(RegistryLocation where, string name, uint quota)
        {
            m_logger.InfoFormat("set Quota for {0}", name);
            try
            {
                using (RegistryKey key = GetRegistryLocation(where).CreateSubKey(name + @"\Software\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    if (quota > 0)
                    {
                        key.SetValue("EnableProfileQuota", 1, RegistryValueKind.DWord);
                        key.SetValue("ProfileQuotaMessage", "\"You have exceeded your profile storage space. Before you can log off, you need to move some items from your profile to network or local storage.\"", RegistryValueKind.String);
                        key.SetValue("MaxProfileSize", quota, RegistryValueKind.DWord);
                        key.SetValue("IncludeRegInProQuota", 1, RegistryValueKind.DWord);
                        key.SetValue("WarnUser", 1, RegistryValueKind.DWord);
                        key.SetValue("WarnUserTimeout", 5, RegistryValueKind.DWord);
                    }
                    else
                    {
                        key.DeleteValue("EnableProfileQuota", false);
                        key.DeleteValue("ProfileQuotaMessage", false);
                        key.DeleteValue("MaxProfileSize", false);
                        key.DeleteValue("IncludeRegInProQuota", false);
                        key.DeleteValue("WarnUser", false);
                        key.DeleteValue("WarnUserTimeout", false);
                    }
                }
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Can't set profile quota for {0} Error:{1}", name, ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean RegQueryQuota(RegistryLocation where, string name)
        {
            m_logger.InfoFormat("query Quota for {0}", name);
            try
            {
                using (RegistryKey key = GetRegistryLocation(where).OpenSubKey(name + @"\Software\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    if (key.GetValue("EnableProfileQuota") == null)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Can't get profile quota for {0} Error:{1}", name, ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean RegRunOnce(RegistryLocation where, string name, string script)
        {
            m_logger.InfoFormat("set RunOnce for {0}", script);
            string[] server;

            if (String.IsNullOrEmpty(script))
            {
                m_logger.DebugFormat("string script is empty");
                return false;
            }
            server = script.Trim('\\').Split('\\');
            if (String.IsNullOrEmpty(server[0]))
            {
                m_logger.DebugFormat("cant extract server address from {0}", script);
                return false;
            }

            try
            {
                using (RegistryKey key = GetRegistryLocation(where).CreateSubKey(name + @"\Software\Microsoft\Windows\CurrentVersion\RunOnce"))
                {
                    key.SetValue("pGina_logonscript", script, RegistryValueKind.String);
                }
                if (!server[0].Contains(":"))
                {
                    using (RegistryKey key = GetRegistryLocation(where).CreateSubKey(name + @"\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Ranges"))
                    {
                        bool found = false;
                        List<string> subkeys = new List<string>();

                        foreach (string subkey in key.GetSubKeyNames())
                        {
                            using (RegistryKey range = key.OpenSubKey(subkey))
                            {
                                if ((range.GetValue(":Range").ToString().Equals(server[0], StringComparison.CurrentCultureIgnoreCase)) && ((range.GetValue("*")) != null))
                                {
                                    m_logger.InfoFormat("subkey {0} contains {1}", subkey, server[0]);
                                    found = true;
                                    break;
                                }
                                subkeys.Add(subkey);
                            }
                        }

                        if (!found)
                        {
                            for (uint x = 1; x < 1000; x++)
                            {
                                if (!subkeys.Contains("Range" + x.ToString()))
                                {
                                    m_logger.InfoFormat("add trustworthy server {0}", server[0]);
                                    using (RegistryKey addrule = key.CreateSubKey("Range" + x.ToString(), RegistryKeyPermissionCheck.ReadWriteSubTree))
                                    {
                                        addrule.SetValue(":Range", server[0], RegistryValueKind.String);
                                        addrule.SetValue("*", 1, RegistryValueKind.DWord);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Can't set RunOnce for {0} Error:{1}", name, ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean RegSec(RegistryLocation where, string name)
        {
            try
            {
                bool result = false;
                m_logger.InfoFormat("set registryrights for {0}", name);
                RegistryAccessRule accessRule = new RegistryAccessRule(name, RegistryRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);

                using (RegistryKey key = GetRegistryLocation(where).OpenSubKey(name, true))
                {
                    RegistrySecurity keySecurity = key.GetAccessControl(AccessControlSections.Access);

                    // delete unknown users
                    try
                    {
                        foreach (RegistryAccessRule user in keySecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                        {
                            m_logger.DebugFormat("ACL user: {0}", user.IdentityReference.Value);
                            if (user.IdentityReference.Value.StartsWith("S-1-5-21-"))
                            {
                                m_logger.DebugFormat("delete unknown user: {0}", user.IdentityReference.Value);
                                keySecurity.RemoveAccessRule(user);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        m_logger.ErrorFormat("delete unknown user error: {0}", ex.Message);
                    }

                    //add the user
                    keySecurity.ModifyAccessRule(AccessControlModification.Add, accessRule, out result);
                    if (result)
                    {
                        key.SetAccessControl(keySecurity);
                    }
                    else
                    {
                        string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                        m_logger.ErrorFormat("Unable to add User {0} {1} {2}", name, result, errorMessage);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("RegSec error {0}", ex.Message);
                return false;
            }

            return true;
        }

    }
}
