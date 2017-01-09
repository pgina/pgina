/*
	Copyright (c) 2017, pGina Team
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
using System.Security.Principal;
using Microsoft.Win32;
using Abstractions.WindowsApi;
using Abstractions.Logging;

namespace Abstractions.Windows
{
    public static class User
    {
        const string ROOT_PROFILE_KEY = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";

        /// <summary>
        /// returns profiledir based on regkey
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        public static List<string> GetProfileDir(SecurityIdentifier sid)
        {
            List<string> ret = new List<string>();

            //"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\S-1-5-21-534125731-1308685933-1530606844-1000}"
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(ROOT_PROFILE_KEY))
            {
                if (key != null)
                {
                    foreach (string keyName in key.GetSubKeyNames())
                    {
                        if (keyName.Contains(sid.ToString())) //get the %SID% and %SID%.bak key
                        {
                            using(RegistryKey subKey = Registry.LocalMachine.OpenSubKey(string.Format("{0}\\{1}", ROOT_PROFILE_KEY, keyName)))
                            {
                                LibraryLogging.Info("ProfileList key found {0}", keyName);
                                ret.Add(subKey.GetValue("ProfileImagePath", "", RegistryValueOptions.None).ToString());
                            }
                        }
                    }
                }
                else
                {
                    LibraryLogging.Info("GetProfileDir key {0} not found", ROOT_PROFILE_KEY);
                }
            }

            return ret;
        }

        internal enum Profile_State
        {
            Profile_is_mandatory = 0x0001,
            Update_the_locally_cached_profile = 0x0002,
            New_local_profile = 0x0004,
            New_central_profile = 0x0008,
            Update_the_central_profile = 0x0010,
            Delete_the_cached_profile = 0x0020,
            Upgrade_the_profile = 0x0040,
            Using_Guest_user_profile = 0x0080,
            Using_Administrator_profile = 0x0100,
            Default_net_profile_is_available_and_ready = 0x0200,
            Slow_network_link_identified = 0x0400,
            Temporary_profile_loaded = 0x0800,
        }
        /// <summary>
        /// return true if profile is temp. uses ProfileList State regkey
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        public static bool? IsProfileTemp(string sid)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(string.Format("{0}\\{1}", ROOT_PROFILE_KEY, sid)))
                {
                    if (key != null)
                    {
                        object type = key.GetValue("State");
                        Profile_State value = 0;
                        switch (key.GetValueKind("State"))
                        {
                            case RegistryValueKind.DWord:
                                value = (Profile_State)type;
                                break;
                            case RegistryValueKind.QWord:
                                value = (Profile_State)type;
                                break;
                        }

                        if (value.HasFlag(Profile_State.Temporary_profile_loaded))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        LibraryLogging.Info("IsProfileTemp key {0}\\{1} not found", ROOT_PROFILE_KEY, sid);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                LibraryLogging.Info("IsProfileTemp exception:{0}", ex.Message);
                return null;
            }

            return false;
        }

        /// <summary>
        /// if a user receives a temp profile of whatever reason
        /// MS is renaming the SID key under ProfileList to SID.bak
        /// if so winapi calls will fail and not returning anything
        /// </summary>
        /// <param name="userSID"></param>
        /// <returns></returns>
        public static Boolean FixProfileList(string userSID)
        {
            string regkey = ROOT_PROFILE_KEY + "\\" + userSID;
            UIntPtr key = UIntPtr.Zero;
            UIntPtr key_bak = UIntPtr.Zero;
            Boolean ret = false;

            key = Abstractions.WindowsApi.pInvokes.RegistryOpenKey(Abstractions.WindowsApi.pInvokes.structenums.baseKey.HKEY_LOCAL_MACHINE, regkey);
            if (key == UIntPtr.Zero)
            {
                key_bak = Abstractions.WindowsApi.pInvokes.RegistryOpenKey(Abstractions.WindowsApi.pInvokes.structenums.baseKey.HKEY_LOCAL_MACHINE, regkey + ".bak");
                if (key_bak != UIntPtr.Zero)
                {
                    key = Abstractions.WindowsApi.pInvokes.RegistryCreateKey(Abstractions.WindowsApi.pInvokes.structenums.baseKey.HKEY_LOCAL_MACHINE, regkey);
                    if (key != UIntPtr.Zero)
                    {
                        ret = Abstractions.WindowsApi.pInvokes.RegistryCopyKey(key_bak, null, key);
                        if (ret)
                        {
                            if (Abstractions.WindowsApi.pInvokes.RegistryCloseKey(key_bak))
                            {
                                Abstractions.WindowsApi.pInvokes.RegistryDeleteTree(Abstractions.WindowsApi.pInvokes.structenums.baseKey.HKEY_LOCAL_MACHINE, regkey + ".bak");
                            }
                        }
                    }
                }
                else
                {
                    ret = true;
                }
            }
            else
            {
                ret = true;
            }
            if (key_bak != UIntPtr.Zero)
            {
                Abstractions.WindowsApi.pInvokes.RegistryCloseKey(key_bak);
            }
            if (key != UIntPtr.Zero)
            {
                Abstractions.WindowsApi.pInvokes.RegistryCloseKey(key);
            }

            return ret;
        }

        /// <summary>
        /// Delete ProfileList regkeys
        /// </summary>
        /// <param name="userSID"></param>
        /// <returns></returns>
        public static Boolean DelProfileList(string userSID)
        {
            List<string> regkeys = new List<string>(){
                ROOT_PROFILE_KEY + "\\" + userSID,
                ROOT_PROFILE_KEY + "\\" + userSID + ".bak"
            };
            UIntPtr key = UIntPtr.Zero;
            Boolean ret = true;

            foreach (string regkey in regkeys)
            {
                key = Abstractions.WindowsApi.pInvokes.RegistryOpenKey(Abstractions.WindowsApi.pInvokes.structenums.baseKey.HKEY_LOCAL_MACHINE, regkey);
                if (key != UIntPtr.Zero)
                {
                    bool r = Abstractions.WindowsApi.pInvokes.RegistryDeleteTree(Abstractions.WindowsApi.pInvokes.structenums.baseKey.HKEY_LOCAL_MACHINE, regkey);
                    if (ret != false)
                    {
                        ret = r;
                    }
                    Abstractions.WindowsApi.pInvokes.RegistryCloseKey(key);
                }
            }

            return ret;
        }

        /// <summary>
        /// returns user profile direrctory
        /// empty string on error
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        public static string GetProfileDir(string username, string password, SecurityIdentifier sid)
        {
            IntPtr hToken = Abstractions.WindowsApi.pInvokes.GetUserToken(username, "", password);
            string ret = "";
            if (hToken != IntPtr.Zero)
            {
                ret = Abstractions.WindowsApi.pInvokes.GetUserProfileDir(hToken);
                Abstractions.WindowsApi.pInvokes.CloseHandle(hToken);
            }
            if (String.IsNullOrEmpty(ret))
            {
                ret = GetProfileDir(sid).DefaultIfEmpty("").FirstOrDefault();
            }

            return ret;
        }

        /// <summary>
        /// sets user profile quota
        /// </summary>
        /// <param name="where">ROOTKEY hklm or hku</param>
        /// <param name="name">SubKey name</param>
        /// <param name="quota">if 0 means the profile quota GPO it will be deleted</param>
        /// <returns>false on error</returns>
        public static Boolean SetQuota(Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation where, string name, uint quota)
        {
            LibraryLogging.Info("set Quota for {0}", name);
            try
            {
                using (RegistryKey key = Abstractions.WindowsApi.pInvokes.GetRegistryLocation(where).CreateSubKey(name + @"\Software\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    if (quota > 0)
                    {
                        key.SetValue("EnableProfileQuota", 1, RegistryValueKind.DWord);
                        //key.SetValue("ProfileQuotaMessage", "You have exceeded your profile storage space. Before you can log off, you need to move some items from your profile to network or local storage.", RegistryValueKind.String);
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
                LibraryLogging.Error("Can't set profile quota for {0} Error:{1}", name, ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// query for already set user profile quota
        /// </summary>
        /// <param name="where"></param>
        /// <param name="name"></param>
        /// <returns>true on already set</returns>
        public static Boolean QueryQuota(Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation where, string name)
        {
            LibraryLogging.Info("query Quota for {0}", name);
            try
            {
                using (RegistryKey key = Abstractions.WindowsApi.pInvokes.GetRegistryLocation(where).OpenSubKey(name + @"\Software\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    if (key.GetValue("EnableProfileQuota") == null)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("Can't get profile quota for {0} Error:{1}", name, ex.Message);
                return false;
            }

            return true;
        }
    }
}
