/*
	Copyright (c) 2016, pGina Team
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
using System.Dynamic;
using System.Security;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace Abstractions.Settings
{
    public class DynamicSettings : DynamicObject
    {
        public static readonly string ROOT_KEY = @"SOFTWARE\pGina3.fork";
        protected string m_rootKey = ROOT_KEY;

        public DynamicSettings()
        {
        }

        public DynamicSettings(string root)
        {
            m_rootKey = root;
        }

        public DynamicSettings(Guid pluginGuid)
        {
            m_rootKey = String.Format(@"{0}\Plugins\{1}", ROOT_KEY, pluginGuid.ToString());
        }

        public DynamicSettings(Guid pluginGuid, string subkey)
        {
            m_rootKey = String.Format(@"{0}\Plugins\{1}\{2}", ROOT_KEY, pluginGuid.ToString(), subkey);
        }

        /// <summary>
        /// Sets the default value for a setting.  Checks to see if the setting
        /// is already defined in the registry.  If so, the method does nothing.
        /// Otherwise the setting is initialized to value.
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <param name="value">The default value for the setting</param>
        public void SetDefault(string name, object value)
        {
            try
            {
                GetSetting(name);
            }
            catch (KeyNotFoundException)
            {
                SetSetting(name, value);
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(m_rootKey, true))
                {
                    if (m_rootKey.EndsWith(@"\global", StringComparison.CurrentCultureIgnoreCase))
                    {
                        System.Security.AccessControl.RegistrySecurity acc = key.GetAccessControl(System.Security.AccessControl.AccessControlSections.All);
                        System.Security.AccessControl.RegistryAccessRule authenticated = new System.Security.AccessControl.RegistryAccessRule(
                            new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.AuthenticatedUserSid, null),
                            System.Security.AccessControl.RegistryRights.ReadKey,
                            System.Security.AccessControl.InheritanceFlags.None,
                            System.Security.AccessControl.PropagationFlags.None,
                            System.Security.AccessControl.AccessControlType.Allow);
                        acc.AddAccessRule(authenticated);
                        key.SetAccessControl(acc);
                    }
                }
            }
        }

        public void SetSetting(string name, object value)
        {
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(m_rootKey))
            {
                key.SetValue(name, value);
            }
        }

        public void SetDefaultEncryptedSetting(string name, string value)
        {
            this.SetDefaultEncryptedSetting(name, value, null);
        }

        public void SetDefaultEncryptedSetting(string name, string value, byte[] optionalEntropy)
        {
            try
            {
                GetEncryptedSetting(name, optionalEntropy);
            }
            catch (KeyNotFoundException)
            {
                SetEncryptedSetting(name, value, optionalEntropy);
            }
        }

        public void SetEncryptedSetting(string name, string value)
        {
            this.SetEncryptedSetting(name, value, null);
        }

        public void SetEncryptedSetting(string name, string value, byte[] optionalEntropy)
        {
            byte[] valueBytes = UnicodeEncoding.Default.GetBytes(value);
            byte[] protectedBytes = ProtectedData.Protect(valueBytes, optionalEntropy, DataProtectionScope.LocalMachine);
            string base64 = Convert.ToBase64String(protectedBytes);
            SetSetting(name, base64);
        }

        public string GetEncryptedSetting(string name)
        {
            return GetEncryptedSetting(name, null);
        }

        public string GetEncryptedSetting(string name, byte[] optionalEntropy)
        {
            string base64 = (string) GetSettingFromRegistry(name);
            byte[] protectedBytes = Convert.FromBase64String(base64);
            byte[] valueBytes = ProtectedData.Unprotect(protectedBytes, optionalEntropy, DataProtectionScope.LocalMachine);
            return UnicodeEncoding.Default.GetString(valueBytes);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            SetSetting(binder.Name, value);
            return true;
        }

        //still dangerous, Exception not catched by calling function
        //FIXME: return null and handle that by calling functions instead of an exception
        private object GetSettingFromRegistry(string name)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(m_rootKey, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl))
            {
                if (key != null)
                {
                    // Make sure key exists before requesting it
                    foreach (string valueName in key.GetValueNames())
                    {
                        if (String.Compare(valueName, name, true) == 0)
                        {
                            return key.GetValue(name);
                        }
                    }

                    throw new KeyNotFoundException(string.Format("Unable to find value for: {0}", name));
                }
                else
                {
                    throw new KeyNotFoundException(string.Format("Unable to open registry key"));
                }
            }
        }

        /// <summary>
        /// Get a dictionary containing all values of the pgina registry
        /// key.  The Dictionary key is the sub-key name, the value is a pGinaDynamicSettings object
        /// corresponding to the sub-key.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetSettings(string[] encypted)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(m_rootKey, false))
                {
                    if (key != null)
                    {
                        string[] names = key.GetValueNames();
                        foreach (string n in names)
                        {
                            object type = key.GetValue(n);
                            string value = "";
                            switch (key.GetValueKind(n))
                            {
                                case RegistryValueKind.String:
                                case RegistryValueKind.ExpandString:
                                    value += type;
                                    break;
                                case RegistryValueKind.Binary:
                                    foreach (byte b in (byte[])type)
                                    {
                                        value += b;
                                    }
                                    Console.WriteLine();
                                    break;
                                case RegistryValueKind.DWord:
                                    value += Convert.ToString((Int32)type);
                                    break;
                                case RegistryValueKind.QWord:
                                    value += Convert.ToString((Int64)type);
                                    break;
                                case RegistryValueKind.MultiString:
                                    foreach (string s in (string[])type)
                                    {
                                        value += String.Format("{0}\0", s);
                                    }
                                    value = value.TrimEnd();
                                    break;
                                default:
                                    break;
                            }

                            if (encypted.Any(s => s.Equals(n, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                value = GetEncryptedSetting(n);
                            }
                            result.Add(n, value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Abstractions.Logging.LibraryLogging.Error("GetSettings({0}, {{1}}) failed:{2}", m_rootKey, String.Join(", ", encypted), ex.Message);
            }

            return result;
        }

        public DynamicSetting GetSetting(string name)
        {
            return new DynamicSetting(name, GetSettingFromRegistry(name));
        }

        public DynamicSetting GetSetting(string name, object def)
        {
            try
            {
                return GetSetting(name);
            }
            catch (KeyNotFoundException)
            {
                return new DynamicSetting(name, def);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetSetting(binder.Name);
            return true;
        }
    }
}
