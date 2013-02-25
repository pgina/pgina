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
using System.Security.AccessControl;
using System.Security;
using System.Security.Principal;
using Microsoft.Win32;
using System.DirectoryServices;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Mail;
using System.Net;

using pGina.Shared.Types;
using log4net;
using log4net.Appender;

namespace pGina.Plugin.pgSMB
{
    public class Roaming
    {
        private static ILog m_logger = LogManager.GetLogger("pgSMB[Roaming]");

        public static BooleanResult get(Dictionary<string,string> settings, string username, string password)
        {
            if (userAdd(settings, username, password, null))
            {
                // user exists and was not created by pgina
                m_logger.InfoFormat("user {0} does already exist on this system and does not contain a comment of \"pGina created pgSMB\"", username);
                return new BooleanResult() { Success = true };
            }
            if (!Connect2share(settings["SMBshare"], username, password, Convert.ToUInt32(settings["ConnectRetry"]), false))
            {
                return new BooleanResult() { Success = false, Message = string.Format("Unable to connect to {0}", settings["RoamingSource"]) };
            }
            try
            {
                if (!Directory.Exists(settings["RoamingSource"]))
                {
                    return new BooleanResult() { Success = false, Message = string.Format("Unable to find Directory {0}", settings["RoamingSource"]) };
                }
                string remote_file = settings["RoamingSource"] + "\\" + settings["Filename"];
                if (File.Exists(remote_file) || File.Exists(remote_file + ".bak"))
                {
                    // there is a remote file
                    Boolean loadprofile = true;
                    // what file to use ?
                    if (File.Exists(remote_file))
                    {
                        settings.Add("Filename_real", settings["Filename"]);
                    }
                    else
                    {
                        settings.Add("Filename_real", settings["Filename"] + ".bak");
                        remote_file += ".bak";
                    }

                    // is there a local roaming profile (there shouldnt be any)
                    if (File.Exists(settings["RoamingDest_real"] + "\\ntuser.dat"))
                    {
                        // there is a local profile of this user
                        // we need to compare the write date between the profile and the compressed remote roaming profile
                        // to be sure that we dont overwrite a newer profile with an old one
                        // possibly reason is a BSOD/hard reset ...
                        m_logger.Debug("User " + username + " still own a lokal profile UTCdate:" + File.GetLastWriteTimeUtc(settings["RoamingDest_real"] + "\\ntuser.dat"));
                        m_logger.Debug("User " + username + " compressed remote profile UTCdate:" + File.GetLastWriteTimeUtc(remote_file));
                        if (DateTime.Compare(File.GetLastWriteTimeUtc(settings["RoamingDest_real"] + "\\ntuser.dat"), File.GetLastWriteTimeUtc(remote_file)) >= 0)
                        {
                            m_logger.Debug("the local profile is newer/equal than the remote one, im not downloading the remote one");
                            loadprofile = false;
                        }
                        else
                        {
                            m_logger.Debug("the local profile is older than the remote one, im removing the local one");
                            if (!DirectoryDel(settings["RoamingDest_real"], Convert.ToUInt32(settings["ConnectRetry"])))
                                m_logger.WarnFormat("Can't delete {0}", settings["RoamingDest_real"]);
                        }
                    }
                    if (!userAdd(settings, username, password, "pGina created pgSMB"))
                    {
                        userDel(settings, username, password);
                        return new BooleanResult() { Success = false, Message = string.Format("Unable to add user {0}", username) };
                    }
                    if (loadprofile)
                    {
                        if (!GetProfile(settings, username, password))
                            return new BooleanResult() { Success = false, Message = string.Format("Unable to get the Profile {0} from {1}", settings["Filename"], settings["RoamingSource"]) };

                        if (!Connect2share(settings["SMBshare"], null, null, 0, true))
                            m_logger.WarnFormat("unable to disconnect from {0}", settings["RoamingSource"]);

                        if (!SetACL(settings, username, password))
                        {
                            userDel(settings, username, password);
                            return new BooleanResult() { Success = false, Message = string.Format("Unable to set ACL for user {0}", username) };
                        }
                    }
                }
                else
                {
                    m_logger.Debug("there is no " + settings["RoamingSource"] + "\\" + settings["Filename"] + " or " + settings["RoamingSource"] + "\\" + settings["Filename"] + ".bak");

                    if (!SetGPO(RegistryLocation.HKEY_USERS, Convert.ToUInt32(settings["MaxStore"]), ".DEFAULT"))
                    {
                        m_logger.WarnFormat("Can't set quota. Thats not terrible");
                    }

                    if (!userAdd(settings, username, password, "pGina created pgSMB"))
                    {
                        userDel(settings, username, password);
                        return new BooleanResult() { Success = false, Message = string.Format("Unable to add user {0}", username) };
                    }
                }
            }
            catch (Exception ex)
            {
                return new BooleanResult() { Success = false, Message = string.Format("Unable to get the Roaming Profile from {0}\nError: {1}", settings["RoamingSource"], ex.Message) };
            }
            finally
            {
                if (!Connect2share(settings["SMBshare"], null, null, 0, true))
                    m_logger.WarnFormat("unable to disconnect from {0}", settings["RoamingSource"]);
            }


            return new BooleanResult() { Success = true };
        }

        public static BooleanResult put(Dictionary<string, string> settings, string username, string password)
        {
            try
            {
                if (!PutProfile(settings, username, password))
                    return new BooleanResult() { Success = false, Message = string.Format("Unable to upload the profile") };
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Error {0} during profile upload", ex.Message);
                return new BooleanResult() { Success = false, Message = string.Format("Unable to upload the profile") };
            }

            return new BooleanResult() { Success = true };
        }

        private static Boolean PutProfile(Dictionary<string, string> settings, string username, string password)
        {
            Int32 cmd = 1;

            //crappy windows cant open 2 connection to the same server
            //we need to fool win to think the server is a different one
            //simply by using IP or FQDN
            string[] server = SMBserver(settings["SMBshare"], true);
            if (!server[0].Equals(server[1]))
            {
                // dont replace any accurance except the first
                server[0] = @"\\" + server[0];
                server[1] = @"\\" + server[1];
                settings["SMBshare"] = settings["SMBshare"].ToLower().Replace(server[0].ToLower(), server[1]);
                settings["RoamingSource"] = settings["RoamingSource"].ToLower().Replace(server[0].ToLower(), server[1]);
                // fooled you
            }

            if (!Directory.Exists(settings["RoamingDest_real"]))
            {
                m_logger.ErrorFormat("Can't find Directory \"{0}\"", settings["RoamingDest_real"]);
                return false;
            }

            if (!File.Exists(settings["RoamingDest_real"]+"\\NTUSER.DAT"))
            {
                m_logger.ErrorFormat("Unable to find \"{0}\" quota exceeded?", settings["RoamingDest_real"] + "\\NTUSER.DAT");
                return false;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            for (uint x = 0; x < Convert.ToUInt32(settings["ConnectRetry"]); x++)
            {
                // run imagex in apply mode
                startInfo.FileName = settings["Compressor"];
                startInfo.Arguments = settings["CompressCLI"];
                m_logger.DebugFormat("Run \"{0}\" \"{1}\"", startInfo.FileName.ToString(), startInfo.Arguments.ToString());
                using (Process p = Process.Start(startInfo))
                {
                    p.WaitForExit();
                    m_logger.Debug("Exitcode:" + p.ExitCode.ToString());
                    cmd = p.ExitCode;
                    if (cmd == 0)
                        break;
                }
            }

            // delete the profile
            if (!DirectoryDel(settings["RoamingDest_real"], Convert.ToUInt32(settings["ConnectRetry"])))
                m_logger.WarnFormat("Can't delete {0}", settings["RoamingDest_real"]);

            //where is the compressed profile now?
            string ThereIsTheProfile = null;
            string[] array = settings["CompressCLI"].Split(' ');
            foreach (string element in array)
            {
                if (element.ToLower().Contains(settings["Filename"].ToLower()))
                {
                    ThereIsTheProfile = element.Trim(new char[] { '"', '\'', '`', ' ', '\t' });
                    m_logger.InfoFormat("The file is stored at {0}", ThereIsTheProfile);
                }
            }
            if (ThereIsTheProfile == null)
            {
                m_logger.ErrorFormat("Unable to find the file \"{0}\" from your compress command {1}", settings["Filename"], settings["CompressCLI"]);
                return false;
            }

            if (cmd == 0)
            {
                if (!Connect2share(settings["SMBshare"], username, password, Convert.ToUInt32(settings["ConnectRetry"]), false))
                {
                    m_logger.ErrorFormat("Unable to connect to {0}", settings["RoamingSource"]);
                    return false;
                }
                if (!Directory.Exists(settings["RoamingSource"]))
                {
                    m_logger.ErrorFormat("Can't find {0}", settings["RoamingSource"]);
                    if (!Connect2share(settings["SMBshare"], null, null, 0, true))
                        m_logger.WarnFormat("unable to disconnect from {0}", settings["RoamingSource"]);
                    return false;
                }

                string remoteFile = settings["RoamingSource"] + "\\" + settings["Filename"];
                string remoteFileBAK = settings["RoamingSource"] + "\\" + settings["Filename"] + ".bak";
                //while (File.Exists(remoteFileBAK) && File.Exists(remoteFile))
                for (uint x = 0; x < Convert.ToUInt32(settings["ConnectRetry"]); x++)
                {
                    // if there is a remote image and a bak image, delete the bak
                    if (File.Exists(remoteFileBAK) && File.Exists(remoteFile))
                    {
                        try
                        {
                            m_logger.DebugFormat("File.Delete {0}", remoteFileBAK);
                            File.Delete(remoteFileBAK);
                        }
                        catch (Exception ex)
                        {
                            m_logger.Debug(ex.Message);
                            Thread.Sleep(1000);
                        }
                    }
                }
                //while (File.Exists(remoteFile))
                for (uint x = 0; x < Convert.ToUInt32(settings["ConnectRetry"]); x++)
                {
                    // if there is a remote wim, rename it to bak
                    if (File.Exists(remoteFile))
                    {
                        try
                        {
                            m_logger.DebugFormat("File.Move {0} {1}", remoteFile, remoteFileBAK);
                            File.Move(remoteFile, remoteFileBAK);
                            File.SetLastWriteTimeUtc(remoteFileBAK, ntp.GetNetworkTime(settings["ntp"]));
                            break;
                        }
                        catch (Exception ex)
                        {
                            m_logger.Debug(ex.Message);
                            Thread.Sleep(1000);
                        }
                    }
                }
                for (uint x = 0; x < Convert.ToUInt32(settings["ConnectRetry"]); x++)
                {
                    // upload the new wim to the smb
                    try
                    {
                        m_logger.DebugFormat("File.Copy {0} {1}", ThereIsTheProfile, remoteFile);
                        File.Copy(ThereIsTheProfile, remoteFile, true);
                        break;
                    }
                    catch (Exception ex)
                    {
                        m_logger.Debug(ex.Message);
                        Thread.Sleep(1000);
                    }
                }
                try
                {
                    // make a timestamp with the current ntp time
                    // a different computer can have a different time/date and so its better to use ntp time
                    File.SetLastWriteTimeUtc(remoteFile, ntp.GetNetworkTime(settings["ntp"]));
                    // cleanup local user
                    m_logger.DebugFormat("File.Delete {0}", ThereIsTheProfile);
                    File.Delete(ThereIsTheProfile);
                }
                catch (Exception ex)
                {
                    m_logger.Debug(ex.Message);
                }
                finally
                {
                    if (!Connect2share(settings["SMBshare"], null, null, 0, true))
                        m_logger.WarnFormat("unable to disconnect from {0}", settings["RoamingSource"]);
                }

                return true;
            }
            return false;
        }

        private static Boolean GetProfile(Dictionary<string,string> settings, string username, string password)
        {
            Int32 cmd = 1;
            m_logger.DebugFormat("User {0} owns a remote profile", username);

            try
            {
                if (!Directory.Exists(settings["RoamingDest_real"]))
                {
                    Directory.CreateDirectory(settings["RoamingDest_real"]);
                }
            }
            catch (Exception ex)
            {
                m_logger.DebugFormat("CreateDirectory({0}) failed {1}", settings["RoamingDest_real"], ex.Message);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            for (uint x = 0; x < Convert.ToUInt32(settings["ConnectRetry"]); x++)
            {
                // run imagex in apply mode
                startInfo.FileName = settings["Compressor"];
                if (settings.ContainsKey("Filename_real"))
                    startInfo.Arguments = settings["UncompressCLI"].Replace(settings["Filename"], settings["Filename_real"]);
                else
                    startInfo.Arguments = settings["UncompressCLI"];
                m_logger.DebugFormat("Run \"{0}\" \"{1}\"", startInfo.FileName.ToString(), startInfo.Arguments.ToString());
                using (Process p = Process.Start(startInfo))
                {
                    p.WaitForExit();
                    m_logger.Debug("Exitcode:" + p.ExitCode.ToString());
                    cmd = p.ExitCode;
                    if (cmd == 0)
                        break;
                }
            }
            if (cmd != 0)
            {
                // Uncompessing failed, clean and disconnect
                // mark the image as tmp and prevent the upload
                DirectoryDel(settings["RoamingDest_real"], Convert.ToUInt32(settings["ConnectRetry"]));
                return false;
            }
            return true;
        }

        public static Boolean userAdd(Dictionary<string,string> settings, string username, string password, string comment)
        {
            UserGroup.USER_INFO_4 userinfo4 = new UserGroup.USER_INFO_4();

            //create user
            if (!UserGroup.UserExists(username))
            {
                if (!UserGroup.UserADD(username, password))
                {
                    m_logger.DebugFormat("Can't add user {0}", username);
                    return false;
                }
            }

            //get userinfo
            if (!UserGroup.UserGet(username, ref userinfo4))
            {
                m_logger.DebugFormat("Can't get userinfo for user {0}", username);
                return false;
            }

            //check if this is a pgina user
            //UserGroup.UserADD will always create a "pGina created pgSMB" comment
            if (comment == null)
                if (!userinfo4.comment.Contains("pGina created pgSMB"))
                    return true;
                else
                    return false;

            //fill userinfo
            if (!String.IsNullOrEmpty(settings["RoamingDest"]))
                userinfo4.profile = settings["RoamingDest"];
            if (!String.IsNullOrEmpty(settings["HomeDir"]))
                userinfo4.home_dir = settings["HomeDir"];
            if (!String.IsNullOrEmpty(settings["HomeDirDrive"]))
                userinfo4.home_dir_drive = settings["HomeDirDrive"];
            if (!String.IsNullOrEmpty(settings["ScriptPath"]))
                userinfo4.script_path = null;
                //userinfo4.script_path = settings["ScriptPath"];
            if (Convert.ToInt32(settings["MaxStore"]) > 0)
                userinfo4.max_storage = Convert.ToInt32(settings["MaxStore"]);
            else
                userinfo4.max_storage = -1;
            userinfo4.password = password;
            if (!String.IsNullOrEmpty(comment))
                userinfo4.comment = comment;
            userinfo4.flags |= UserGroup.UserFlags.UF_NORMAL_ACCOUNT;
            userinfo4.acct_expires = -1;
            userinfo4.logon_hours = IntPtr.Zero;

            //apply userinfo
            if (!UserGroup.UserMod(username, userinfo4))
            {
                m_logger.DebugFormat("Can't modify user {0}", username);
                return false;
            }
            m_logger.InfoFormat("user {0} created", username);
            return true;
        }

        public static Boolean userDel(Dictionary<string, string> settings, string username, string password)
        {
            if (!UserGroup.UserDel(username))
                m_logger.WarnFormat("Can't delete userAccount {0}", username);
            if (Directory.Exists(settings["RoamingDest_real"]))
            {
                DirectoryDel(settings["RoamingDest_real"], Convert.ToUInt32(settings["ConnectRetry"]));
            }

            return true;
        }

        public static Boolean DirectoryDel(string path, uint retry)
        {
            if (Directory.Exists(path))
            {
                for (uint x = 0; x < retry; x++)
                {
                    try
                    {
                        Directory.Delete(path, true);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        m_logger.WarnFormat("unable to delete {0} error {1}", path, ex.Message);
                        if (!AddDirectorySecurity(path, WindowsIdentity.GetCurrent().Name, FileSystemRights.FullControl, AccessControlType.Allow, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None))
                            m_logger.DebugFormat("failed to set DirectorySecurity for {0} at {1}", WindowsIdentity.GetCurrent().Name, path);
                        else
                            if (!SetAttrib(new DirectoryInfo(path)))
                                m_logger.DebugFormat("failed to set Attributes at {0}", path);
                    }
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        public static Boolean SetGPO(RegistryLocation regkey, UInt32 maxStore, string username)
        {
            if (!registry.RegQuota(regkey, username, maxStore))
            {
                return false;
            }
            return true;
        }

        private static Boolean SetACL(Dictionary<string, string> settings, string username, string password)
        {
            if (!AddDirectorySecurity(settings["RoamingDest_real"], username, FileSystemRights.FullControl, AccessControlType.Allow, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None))
            {
                m_logger.WarnFormat("Can't set ACL for Directory {0}", settings["RoamingDest_real"]);
                return false;
            }
            if (!registry.RegistryLoad("HKEY_LOCAL_MACHINE", username, settings["RoamingDest_real"] + "\\NTUSER.DAT"))
            {
                m_logger.WarnFormat("Can't load regfile {0}", settings["RoamingDest_real"] + "\\NTUSER.DAT");
                return false;
            }
            if (!registry.RegSec(RegistryLocation.HKEY_LOCAL_MACHINE, username))
            {
                m_logger.WarnFormat("Can't set ACL for regkey {0}\\{1}", RegistryLocation.HKEY_LOCAL_MACHINE.ToString(), username);
                return false;
            }
            if (!SetGPO(RegistryLocation.HKEY_LOCAL_MACHINE, Convert.ToUInt32(settings["MaxStore"]), username))
            {
                m_logger.WarnFormat("Can't set quota. Thats not terrible");
            }
            if (!registry.RegistryUnLoad("HKEY_LOCAL_MACHINE", username))
            {
                m_logger.WarnFormat("Can't unload regkey {0}\\{1}", settings["RoamingDest_real"], "NTUSER.DAT");
                return false;
            }

            return true;
        }

        private static Boolean SetAttrib(DirectoryInfo dir)
        {
            try
            {
                foreach (DirectoryInfo subDirPath in dir.GetDirectories())
                {
                    subDirPath.Attributes = FileAttributes.Normal;
                    SetAttrib(subDirPath);
                }
                foreach (FileInfo filePath in dir.GetFiles())
                    filePath.Attributes = FileAttributes.Normal;
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Cant't set Attrib normal on {0} Error:{1}", dir, ex.Message );
                return false;
            }

            return true;
        }

        private static Boolean AddDirectorySecurity(string Directory, string Account, FileSystemRights Rights, AccessControlType ControlType, InheritanceFlags Inherit, PropagationFlags Propagation)
        {
            DirectoryInfo dInfo = new DirectoryInfo(Directory);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            try
            {
                foreach (FileSystemAccessRule user in dSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                {
                    m_logger.Debug("ACL user:" + user.IdentityReference.Value);
                    if (user.IdentityReference.Value.StartsWith("S-1-5-21-"))
                    {
                        m_logger.Debug("delete unknown user:" + user.IdentityReference.Value);
                        dSecurity.RemoveAccessRule(user);
                    }
                }
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Unable to GetAccessRules for {0} error {1}", Directory, ex.Message);
                return false;
            }

            dSecurity.AddAccessRule(new FileSystemAccessRule(Account, Rights, Inherit, Propagation, ControlType));
            try
            {
                dSecurity.SetOwner(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Unable to SetAccessControl for {0} error {1}", Directory, ex.Message);
                return false;
            }
            return true;
        }

        private static string[] SMBserver(string share, Boolean visversa)
        {
            string[] ret = { null, null };
            string[] server;
            try
            {
                server = share.Trim('\\').Split('\\');
            }
            catch { return ret; }

            if (!String.IsNullOrEmpty(server[0]))
            {
                ret[0] = server[0];
                ret[1] = server[0];
                if (!visversa)
                    return ret;
                try
                {
                    IPHostEntry hostFQDN = Dns.GetHostEntry(server[0]);
                    if (hostFQDN.HostName.Equals(server[0], StringComparison.CurrentCultureIgnoreCase))
                    {
                        IPAddress[] hostIPs = Dns.GetHostAddresses(server[0]);
                        ret[1] = hostIPs[0].ToString();
                    }
                    else
                    {
                        ret[1] = hostFQDN.HostName;
                    }
                }
                catch { }
            }

            return ret;
        }

        private static Boolean Connect2share(string share, string username, string password, uint retry, Boolean DISconnect)
        {
            if (DISconnect)
            {
                m_logger.DebugFormat("Disconnect from {0}", share);
                if (!unc.disconnectRemote(share))
                    m_logger.WarnFormat("unable to disconnect from {0}", share);

                return true;
            }
            else
            {
                string[] server = SMBserver(share, false);
                if (String.IsNullOrEmpty(server[0]))
                {
                    m_logger.ErrorFormat("Can't extract SMB server from {0}", share);
                    return false;
                }

                server[0] += "\\"+username;

                for (int x = 1; x <= retry; x++)
                {
                    try
                    {
                        m_logger.DebugFormat("{0}. try to connect to {1} as {2}", x, share, server[0]);
                        if (!unc.connectToRemote(share, server[0], password))
                            m_logger.ErrorFormat("Failed to connect to share {0}", share);
                        if (Directory.Exists(share))
                            return true;
                    }
                    catch (Exception ex)
                    {
                        m_logger.Error(ex.Message);
                    }
                }

                return false;
            }
        }

        private static Boolean AddDirectorySecurity(string Directory, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            DirectoryInfo dInfo = new DirectoryInfo(Directory);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            try
            {
                foreach (FileSystemAccessRule user in dSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                {
                    m_logger.DebugFormat("ACL user:{0}", user.IdentityReference.Value);
                    if (user.IdentityReference.Value.StartsWith("S-1-5-21-"))
                    {
                        m_logger.DebugFormat("delete unknown user: {0}", user.IdentityReference.Value);
                        dSecurity.RemoveAccessRule(user);
                    }
                }
            }
            catch (Exception ex)
            {
                m_logger.DebugFormat("RemoveAccessRule failed for {0} with error {1}", Directory, ex.Message);
                return false;
            }

            dSecurity.AddAccessRule(new FileSystemAccessRule(Account, Rights, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, ControlType));
            try
            {
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                m_logger.DebugFormat("SetAccessControl for {0} failed with error {1}",Directory ,ex.Message);
                return false;
            }

            return true;
        }

        public static Boolean email(string mailAddress, string smtpAddress, string username, string password, string subject, string body)
        {
            Boolean ret = true;

            if (String.IsNullOrEmpty(mailAddress))
                return false;
            if (String.IsNullOrEmpty(smtpAddress))
                return false;

            try
            {
                using (EventLog systemLog = new EventLog("System"))
                {
                    body += "\n\n====================Eventlog System====================\n";
                    for (int x = systemLog.Entries.Count - 30; x < systemLog.Entries.Count; x++)
                    {
                        body += String.Format("{0} {1} {2} {3}\n", systemLog.Entries[x].TimeGenerated, systemLog.Entries[x].EntryType, (UInt16)systemLog.Entries[x].InstanceId, systemLog.Entries[x].Message);
                    }
                }
                using (EventLog application = new EventLog("Application"))
                {
                    body += "\n\n====================Eventlog Application===============\n";
                    for (int x = application.Entries.Count - 30; x < application.Entries.Count; x++)
                    {
                        body += String.Format("{0} {1} {2} {3}\n", application.Entries[x].TimeGenerated, application.Entries[x].EntryType, (UInt16)application.Entries[x].InstanceId, application.Entries[x].Message);
                    }
                }
            }
            catch { }

            string[] mails = mailAddress.Split(' ');
            string[] smtps = smtpAddress.Split(' ');

            for (uint x = 0; x < smtps.Length; x++)
            {
                using (SmtpClient client = new SmtpClient(smtps[x]))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(username, password);

                    for (uint y = 0; y < mails.Length; y++)
                    {
                        if (mails[y] == null)
                            continue;
                        try
                        {
                            // get the logfile
                            string logfile = null;
                            logfile = LogManager.GetRepository().GetAppenders().OfType<FileAppender>().Where(fa => fa.Name == "bigfile").Single().File;
                            if (!String.IsNullOrEmpty(logfile))
                            {
                                using (StreamReader log = new StreamReader(logfile, true))
                                {
                                    // read the last 50kbytes of the log
                                    if (log.BaseStream.Length > 50 * 1024) //50kbytes
                                        log.BaseStream.Seek(50 * 1024 * -1, SeekOrigin.End);

                                    string[] lastlines = log.ReadToEnd().Split('\n');
                                    int line_count = 0;
                                    if (lastlines.Length > 50)
                                        line_count = lastlines.Length - 51;
                                    body += "\n\n====================Pgina log==========================\n";
                                    for (; line_count < lastlines.Length; line_count++)
                                    {
                                        body += lastlines[line_count] + '\n';
                                    }
                                }
                            }

                            MailMessage message = new MailMessage(mails[y], mails[y], subject, body);

                            client.Send(message);
                            mails[y] = null;
                        }
                        catch (Exception ex)
                        {
                            m_logger.WarnFormat("Failed to send message \"{0}\" to {1} Error:{2}", subject, mails[y], ex.Message);
                            ret = false;
                        }
                    }
                }

                if (mails.All(k => string.IsNullOrEmpty(k)))
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }
    }
}
