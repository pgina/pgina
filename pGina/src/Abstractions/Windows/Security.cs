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

using Abstractions.Logging;
using Abstractions.WindowsApi;
using System.Security.AccessControl;
using Microsoft.Win32;
using System.IO;

namespace Abstractions.Windows
{
    public static class Security
    {
        public static SecurityIdentifier GetWellknownSID(WellKnownSidType type)
        {
            return new SecurityIdentifier(type, null);
        }

        public static string GetWellKnownName(WellKnownSidType type)
        {
            return GetNameFromSID(GetWellknownSID(type));
        }

        public static string GetNameFromSID(SecurityIdentifier sid)
        {
            NTAccount ntAccount = (NTAccount)sid.Translate(typeof(NTAccount));
            return ntAccount.ToString();
        }

        public static SecurityIdentifier GetSIDFromName(string name)
        {
            NTAccount ntAccount = new NTAccount(name);
            return (SecurityIdentifier)ntAccount.Translate(typeof(SecurityIdentifier));
        }

        /// <summary>
        /// apply registry security settings to user profiles
        /// </summary>
        /// <param name="where"></param>
        /// <param name="keyname"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static Boolean RegSec(pInvokes.structenums.RegistryLocation where, string keyname, string username)
        {
            try
            {
                IdentityReference UserIRef = new NTAccount(String.Format("{0}\\{1}", Environment.MachineName, username));
                SecurityIdentifier UserSid = (SecurityIdentifier)UserIRef.Translate(typeof(SecurityIdentifier));

                using (RegistryKey key = pInvokes.GetRegistryLocation(where).OpenSubKey(keyname, true))
                {
                    RegistrySecurity keySecurity = key.GetAccessControl(AccessControlSections.Access);
                    string SDDL = keySecurity.GetSecurityDescriptorSddlForm(AccessControlSections.All);
                    //LibraryLogging.Info(SDDL);

                    foreach (RegistryAccessRule user in keySecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                    {
                        //LibraryLogging.Info("registry ACE user: {0} {1} {2}", key.Name, user.InheritanceFlags.ToString(), user.IdentityReference.Value);
                        if (user.IdentityReference.Value.StartsWith("S-1-5-21-") && !user.IdentityReference.Value.Equals(UserIRef.Value))
                        {
                            //LibraryLogging.Info("mod registry ACE:{0} from unknown user:{1} to {2} {3} {4}", key.Name, user.IdentityReference.Value, username, user.RegistryRights.ToString(), user.AccessControlType.ToString());

                            SDDL = SDDL.Replace(user.IdentityReference.Value, UserSid.Value);
                            //LibraryLogging.Info(SDDL);
                            keySecurity.SetSecurityDescriptorSddlForm(SDDL);
                            key.SetAccessControl(keySecurity);

                            break;
                        }
                    }
                    foreach (string subkey in key.GetSubKeyNames())
                    {
                        if (!RegSec(where, keyname + "\\" + subkey, username))
                        {
                            return false;
                        }
                    }
                }
            }
            catch (SystemException ex)
            {
                LibraryLogging.Warn("RegSec:{0} Warning {1}", keyname, ex.Message);
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("RegSec:{0} Error:{1}", keyname, ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// apply recursive attribute to directories and files
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="attrib"></param>
        /// <returns></returns>
        public static Boolean SetRecDirAttrib(DirectoryInfo dir, FileAttributes attrib)
        {
            try
            {
                foreach (DirectoryInfo subDirPath in dir.GetDirectories())
                {
                    subDirPath.Attributes = attrib;
                    SetRecDirAttrib(subDirPath, attrib);
                }
                foreach (FileInfo filePath in dir.GetFiles())
                {
                    filePath.Attributes = attrib;
                }
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("Cant't set attrib {0} on {1} Error:{2}", attrib, dir, ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// remove users who are unknown to the system from the directory acl
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static Boolean RemoveAccRuleFromUnknownUser(string dir)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dir);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            try
            {
                foreach (FileSystemAccessRule user in dSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                {
                    //LibraryLogging.Debug("directory ACE user: {0}", user.IdentityReference.Value);
                    if (user.IdentityReference.Value.StartsWith("S-1-5-21-"))
                    {
                        LibraryLogging.Debug("delete unknown directory ACE from {0} in {1}", user.IdentityReference.Value, dir);
                        dSecurity.RemoveAccessRule(user);
                    }
                }
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("unable to RemoveAccRuleFromUnknownUser for {0} error {1}", dir, ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean SetDirectorySecurity(string dir, IdentityReference[] Account, FileSystemRights Rights, AccessControlType ControlType, InheritanceFlags Inherit, PropagationFlags Propagation)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dir);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            try
            {
                foreach (IdentityReference account in Account)
                {
                    dSecurity.AddAccessRule(new FileSystemAccessRule(account, Rights, Inherit, Propagation, ControlType));
                }
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("unable to SetDirectorySecurity for {0} error {1}", dir, ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean SetDirectorySecurity(string dir, string[] Account, FileSystemRights Rights, AccessControlType ControlType, InheritanceFlags Inherit, PropagationFlags Propagation)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dir);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            try
            {
                foreach (string account in Account)
                {
                    dSecurity.AddAccessRule(new FileSystemAccessRule(account, Rights, Inherit, Propagation, ControlType));
                }
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("unable to SetDirectorySecurity for {0} error {1}", dir, ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean ReplaceDirectorySecurity(string dir, IdentityReference[] Account, FileSystemRights Rights, AccessControlType ControlType, InheritanceFlags Inherit, PropagationFlags Propagation)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dir);
            DirectorySecurity dSecurity = new DirectorySecurity();

            try
            {
                dSecurity.SetAccessRuleProtection(true, false);
                foreach (IdentityReference account in Account)
                {
                    dSecurity.ResetAccessRule(new FileSystemAccessRule(account, Rights, Inherit, Propagation, ControlType));
                }
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("unable to ReplaceDirectorySecurity for {0} error {1}", dir, ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean ReplaceDirectorySecurity(string dir, string[] Account, FileSystemRights Rights, AccessControlType ControlType, InheritanceFlags Inherit, PropagationFlags Propagation)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dir);
            DirectorySecurity dSecurity = new DirectorySecurity();

            try
            {
                dSecurity.SetAccessRuleProtection(true, false);
                foreach (string account in Account)
                {
                    dSecurity.ResetAccessRule(new FileSystemAccessRule(account, Rights, Inherit, Propagation, ControlType));
                }
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("unable to ReplaceDirectorySecurity for {0} error {1}", dir, ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean ReplaceFileSecurity(string File, IdentityReference[] Account, FileSystemRights Rights, AccessControlType ControlType, InheritanceFlags Inherit, PropagationFlags Propagation)
        {
            FileInfo fInfo = new FileInfo(File);
            FileSecurity fSecurity = fInfo.GetAccessControl();

            try
            {
                fSecurity.SetAccessRuleProtection(true, false);
                foreach (IdentityReference account in Account)
                {
                    fSecurity.ResetAccessRule(new FileSystemAccessRule(account, Rights, Inherit, Propagation, ControlType));
                }
                fInfo.SetAccessControl(fSecurity);
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("unable to ReplaceFileSecurity for {0} error {1}", File, ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean ReplaceFileSecurity(string File, string[] Account, FileSystemRights Rights, AccessControlType ControlType, InheritanceFlags Inherit, PropagationFlags Propagation)
        {
            FileInfo fInfo = new FileInfo(File);
            FileSecurity fSecurity = fInfo.GetAccessControl();

            try
            {
                fSecurity.SetAccessRuleProtection(true, false);
                foreach (string account in Account)
                {
                    fSecurity.ResetAccessRule(new FileSystemAccessRule(account, Rights, Inherit, Propagation, ControlType));
                }
                fInfo.SetAccessControl(fSecurity);
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("unable to ReplaceFileSecurity for {0} error {1}", File, ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean SetDirOwner(string dir, IdentityReference Account)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dir);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            try
            {
                dSecurity.SetOwner(Account);
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("SetDirOwner unable to SetOwner for {0} error {1}", dir, ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean SetDirOwner(string dir, string Account)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dir);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            IdentityReference User = new NTAccount(String.Format("{0}\\{1}",Environment.MachineName, Account));

            try
            {
                dSecurity.SetOwner(User);
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                LibraryLogging.Error("SetDirOwner unable to SetOwner for {0} error {1}", dir, ex.Message);
                return false;
            }

            return true;
        }
    }
}
