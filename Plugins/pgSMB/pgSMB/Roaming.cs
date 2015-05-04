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
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Net;

using Abstractions;
using pGina.Shared.Types;
using log4net;

namespace pGina.Plugin.pgSMB
{
    public class Roaming
    {
        private static ILog m_logger = LogManager.GetLogger("pgSMB[Roaming]");

        public static BooleanResult get(Dictionary<string,string> settings, string username, string password)
        {
            if (!UserCanBeUsed(username))
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
                    try
                    {
                        Directory.CreateDirectory(settings["RoamingSource"]);
                    }
                    catch (Exception ex)
                    {
                        m_logger.DebugFormat("CreateDirectory({0}) failed {1}", settings["RoamingSource"], ex.Message);
                    }
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
                    string ProfDir = GetExistingUserProfile(username, password);
                    if (File.Exists(ProfDir + "\\ntuser.dat")) //worst case "\\ntuser.dat"
                    {
                        // there is a local profile of this user
                        // we need to compare the write date between the profile and the compressed remote roaming profile
                        // to be sure that we dont overwrite a newer profile with an old one
                        // possibly reason is a BSOD/hard reset ...
                        m_logger.Debug("User " + username + " still own a lokal profile UTCdate:" + File.GetLastWriteTimeUtc(ProfDir + "\\ntuser.dat"));
                        m_logger.Debug("User " + username + " compressed remote profile UTCdate:" + File.GetLastWriteTimeUtc(remote_file));
                        if (DateTime.Compare(File.GetLastWriteTimeUtc(ProfDir + "\\ntuser.dat"), File.GetLastWriteTimeUtc(remote_file)) >= 0)
                        {
                            m_logger.DebugFormat("the local profile ('{0}') is newer/equal than the remote one, im not downloading the remote one", ProfDir);
                            loadprofile = false;
                        }
                        else
                        {
                            m_logger.Debug("the local profile is older than the remote one");
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
                        {
                            return new BooleanResult() { Success = false, Message = string.Format("Unable to get the Profile {0} from {1}", settings["Filename"], settings["RoamingSource"]) };
                        }

                        if (!Connect2share(settings["SMBshare"], null, null, 0, true))
                        {
                            m_logger.WarnFormat("unable to disconnect from {0}", settings["RoamingSource"]);
                        }

                        if (!SetACL(settings["RoamingDest_real"], username, password, Convert.ToUInt32(settings["MaxStore"]), Convert.ToUInt32(settings["ConnectRetry"])))
                        {
                            userDel(settings, username, password);
                            return new BooleanResult() { Success = false, Message = string.Format("Unable to set ACL for user {0}", username) };
                        }
                    }
                }
                else
                {
                    m_logger.DebugFormat("there is no {0}\\{1} or {2}\\{3}{4}", settings["RoamingSource"], settings["Filename"], settings["RoamingSource"], settings["Filename"], ".bak");

                    if (!userAdd(settings, username, password, "pGina created pgSMB"))
                    {
                        userDel(settings, username, password);
                        return new BooleanResult() { Success = false, Message = string.Format("Unable to add user {0}", username) };
                    }
                    if (Directory.Exists(settings["RoamingDest_real"]))
                    {
                        DirectoryDel(settings["RoamingDest_real"], Convert.ToUInt32(settings["ConnectRetry"]));
                    }
                    if (!Roaming.CreateRoamingFolder(settings["RoamingDest_real"], username))
                    {
                        return new BooleanResult() { Success = false, Message = string.Format("Unable to create the Roaming folder {0}", settings["RoamingDest_real"]) };
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
                {
                    m_logger.WarnFormat("unable to disconnect from {0}", settings["RoamingSource"]);
                }
            }


            return new BooleanResult() { Success = true };
        }

        public static BooleanResult put(Dictionary<string, string> settings, string username, string password)
        {
            try
            {
                if (!PutProfile(settings, username, password))
                {
                    return new BooleanResult() { Success = false, Message = string.Format("Unable to upload the profile") };
                }
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
            int ret_code = -1;

            //crappy windows cant open 2 connection to the same server
            //we need to fool win to think the server is a different one
            //simply by using IP or FQDN
            string[] server = {null, null};
            for (uint x = 0; x < Convert.ToUInt32(settings["ConnectRetry"]); x++)
            {
                server = SMBserver(settings["SMBshare"], true);
                if (!String.IsNullOrEmpty(server[0]) && !String.IsNullOrEmpty(server[1]))
                {
                    break;
                }
                Thread.Sleep(new TimeSpan(0, 0, 3));
            }
            if (String.IsNullOrEmpty(server[0]) || String.IsNullOrEmpty(server[1]))
            {
                m_logger.InfoFormat("can't resolve IP or FQDN from {0} I will try to continue, but the upload my fail with System Error 1219", settings["SMBshare"]);
            }
            else
            {
                if (!server[0].Equals(server[1]))
                {
                    // dont replace any accurance except the first
                    server[0] = @"\\" + server[0];
                    server[1] = @"\\" + server[1];
                    settings["SMBshare"] = settings["SMBshare"].ToLower().Replace(server[0].ToLower(), server[1]);
                    settings["RoamingSource"] = settings["RoamingSource"].ToLower().Replace(server[0].ToLower(), server[1]);
                    // fooled you
                }
                else
                {
                    m_logger.InfoFormat("can't fool windows to think {0} is a different server. I will try to continue but the upload my fail with System Error 1219", server[0]);
                }
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

            for (uint x = 0; x < Convert.ToUInt32(settings["ConnectRetry"]); x++)
            {
                // run imagex in capture mode
                string stdmerge = "";

                m_logger.DebugFormat("Run \"{0}\" \"{1}\"", settings["Compressor"], settings["CompressCLI"]);
                ret_code = RunWait(settings["Compressor"], settings["CompressCLI"], out stdmerge);
                if (ret_code == 0)
                {
                    break;
                }
                m_logger.DebugFormat("Exitcode:{0}\n{1}", ret_code, tail(stdmerge, 10));
                Thread.Sleep(new TimeSpan(0, 0, 30));
            }

            // delete the profile
            if (!DirectoryDel(settings["RoamingDest_real"], Convert.ToUInt32(settings["ConnectRetry"])))
            {
                m_logger.WarnFormat("Can't delete {0}", settings["RoamingDest_real"]);
            }

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
            if (String.IsNullOrEmpty(ThereIsTheProfile))
            {
                m_logger.ErrorFormat("Unable to find the file \"{0}\" in your compress command {1}", settings["Filename"], settings["CompressCLI"]);
                return false;
            }

            if (ret_code == 0)
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
                    {
                        m_logger.WarnFormat("unable to disconnect from {0}", settings["RoamingSource"]);
                    }
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
                            Thread.Sleep(new TimeSpan(0, 0, 1));
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
                            break;
                        }
                        catch (Exception ex)
                        {
                            m_logger.Debug(ex.Message);
                            Thread.Sleep(new TimeSpan(0, 0, 1));
                        }
                    }
                }

                // check share space
                long wimbak_size = 0;
                long wim_size = 0;
                if (File.Exists(remoteFileBAK))
                {
                    FileInfo fwimbak = new FileInfo(remoteFileBAK);
                    wimbak_size = fwimbak.Length;
                }
                FileInfo fwim = new FileInfo(ThereIsTheProfile);
                wim_size = fwim.Length;
                long[] freespace = Abstractions.WindowsApi.pInvokes.GetFreeShareSpace(settings["SMBshare"]);
                if (freespace[0] > -1)
                {
                    if (wim_size > freespace[0])
                    {
                        if ((wim_size - wimbak_size) < freespace[0])
                        {
                            m_logger.InfoFormat("I'll store the bak file at {0} instead of {1}, because there is not enough space on {2} {3} bytes", ThereIsTheProfile + ".bak", remoteFileBAK, settings["SMBshare"], freespace);
                            try
                            {
                                m_logger.InfoFormat("File.Copy {0} {1}", remoteFileBAK, ThereIsTheProfile + ".bak");
                                File.Copy(remoteFileBAK, ThereIsTheProfile + ".bak", true);
                                m_logger.InfoFormat("File.Delete {0}", remoteFileBAK);
                                File.Delete(remoteFileBAK);
                                Abstractions.Windows.Security.ReplaceFileSecurity(ThereIsTheProfile + ".bak", new IdentityReference[] { new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null) }, FileSystemRights.FullControl, AccessControlType.Allow, InheritanceFlags.None, PropagationFlags.None);
                            }
                            catch (Exception ex)
                            {
                                m_logger.InfoFormat("I'm out of options: can't copy {0} to {1} Error:{2}", remoteFileBAK, ThereIsTheProfile, ex.Message);
                            }
                        }
                        else
                        {
                            m_logger.InfoFormat("not enough space on {0} to store {1} with size {2}", settings["SMBshare"], ThereIsTheProfile, wim_size);
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
                    }

                    if (x == Convert.ToUInt32(settings["ConnectRetry"]) - 1)
                    {
                        if (!Connect2share(settings["SMBshare"], null, null, 0, true))
                        {
                            m_logger.WarnFormat("unable to disconnect from {0}", settings["RoamingSource"]);
                        }
                        Abstractions.Windows.Security.ReplaceFileSecurity(ThereIsTheProfile, new IdentityReference[] { new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null) }, FileSystemRights.FullControl, AccessControlType.Allow, InheritanceFlags.None, PropagationFlags.None);
                        return false;
                    }
                    else
                    {
                        Thread.Sleep(new TimeSpan(0, 0, 1));
                    }
                }
                try
                {
                    // make a timestamp with the current ntp time
                    // a different computer can have a different time/date and so its better to use ntp time
                    DateTime ntpTime = Abstractions.Windows.Networking.GetNetworkTime(settings["ntp"].Split(' '));
                    if (ntpTime != DateTime.MinValue)
                    {
                        File.SetLastWriteTimeUtc(remoteFile, ntpTime);
                    }
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
                    {
                        m_logger.WarnFormat("unable to disconnect from {0}", settings["RoamingSource"]);
                    }
                }

                return true;
            }
            return false;
        }

        private static Boolean GetProfile(Dictionary<string,string> settings, string username, string password)
        {
            int ret_code = -1;
            m_logger.DebugFormat("User {0} owns a remote profile", username);

            if (Directory.Exists(settings["RoamingDest_real"]))
            {
                DirectoryDel(settings["RoamingDest_real"], Convert.ToUInt32(settings["ConnectRetry"]));
            }

            if (!CreateRoamingFolder(settings["RoamingDest_real"], username))
            {
                m_logger.ErrorFormat("failed to create the roaming directory {0}", settings["RoamingDest_real"]);
                return false;
            }

            for (uint x = 0; x < Convert.ToUInt32(settings["ConnectRetry"]); x++)
            {
                // run imagex in apply mode
                string stdmerge = "";
                string args = "";

                if (settings.ContainsKey("Filename_real"))
                {
                    args = settings["UncompressCLI"].Replace(settings["Filename"], settings["Filename_real"]);
                }
                else
                {
                    args = settings["UncompressCLI"];
                }

                m_logger.DebugFormat("Run \"{0}\" \"{1}\"", settings["Compressor"], args);
                ret_code = RunWait(settings["Compressor"], args, out stdmerge);
                if (ret_code == 0)
                {
                    break;
                }
                m_logger.DebugFormat("Exitcode:{0}\n{1}", ret_code, tail(stdmerge,10));
                Thread.Sleep(new TimeSpan(0, 0, 30));
            }

            if (ret_code != 0)
            {
                // Uncompessing failed, clean and disconnect
                // mark the image as tmp and prevent the upload
                DirectoryDel(settings["RoamingDest_real"], Convert.ToUInt32(settings["ConnectRetry"]));
                return false;
            }

            return true;
        }

        private static string GetExistingUserProfile(string username, string password)
        {
            Abstractions.WindowsApi.pInvokes.structenums.USER_INFO_4 userinfo4 = new Abstractions.WindowsApi.pInvokes.structenums.USER_INFO_4();
            if (!Abstractions.WindowsApi.pInvokes.UserGet(username, ref userinfo4))
            {
                m_logger.DebugFormat("Can't get userinfo for user {0}", username);
                return "";
            }
            string userSID = new SecurityIdentifier(userinfo4.user_sid).Value;
            if (!Abstractions.Windows.User.FixProfileList(userSID))
            {
                m_logger.DebugFormat("Error in FixProfileList {0}", userSID);
            }

            return Abstractions.Windows.User.GetProfileDir(username, password);
        }

        public static Boolean userAdd(Dictionary<string,string> settings, string username, string password, string comment)
        {
            Abstractions.WindowsApi.pInvokes.structenums.USER_INFO_4 userinfo4 = new Abstractions.WindowsApi.pInvokes.structenums.USER_INFO_4();

            //create user
            if (!Abstractions.WindowsApi.pInvokes.UserExists(username))
            {
                if (!Abstractions.WindowsApi.pInvokes.UserADD(username, password, comment))
                {
                    m_logger.DebugFormat("Can't add user {0}", username);
                    return false;
                }
            }

            //get userinfo
            if (!Abstractions.WindowsApi.pInvokes.UserGet(username, ref userinfo4))
            {
                m_logger.DebugFormat("Can't get userinfo for user {0}", username);
                return false;
            }

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
            /*if (Convert.ToInt32(settings["MaxStore"]) > 0)
                userinfo4.max_storage = Convert.ToInt32(settings["MaxStore"]);
            else
                userinfo4.max_storage = -1;*/
            userinfo4.password = password;
            userinfo4.comment = comment;
            userinfo4.flags |= Abstractions.WindowsApi.pInvokes.structenums.UserFlags.UF_NORMAL_ACCOUNT;
            userinfo4.acct_expires = -1;
            userinfo4.logon_hours = IntPtr.Zero;

            //apply userinfo
            if (!Abstractions.WindowsApi.pInvokes.UserMod(username, userinfo4))
            {
                m_logger.DebugFormat("Can't modify user {0}", username);
                return false;
            }
            m_logger.InfoFormat("user {0} created", username);

            return true;
        }

        public static Boolean UserCanBeUsed(string username)
        {
            Abstractions.WindowsApi.pInvokes.structenums.USER_INFO_4 userinfo4 = new Abstractions.WindowsApi.pInvokes.structenums.USER_INFO_4();

            //get userinfo
            if (!Abstractions.WindowsApi.pInvokes.UserGet(username, ref userinfo4))
            {
                return true;
            }

            //check if this is a pgina user
            if (userinfo4.comment.Contains("pGina created pgSMB"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Boolean userDel(Dictionary<string, string> settings, string username, string password)
        {
            if (!Abstractions.WindowsApi.pInvokes.UserDel(username))
            {
                m_logger.WarnFormat("Can't delete userAccount {0}", username);
            }
            if (Directory.Exists(settings["RoamingDest_real"]))
            {
                DirectoryDel(settings["RoamingDest_real"], Convert.ToUInt32(settings["ConnectRetry"]));
            }

            return true;
        }

        public static Boolean DirectoryDel(string path, uint retry)
        {
            for (uint x = 1; x <= retry; x++)
            {
                if (Directory.Exists(path))
                {
                    try
                    {
                        Directory.Delete(path, true);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        if (x == retry)
                        {
                            m_logger.WarnFormat("unable to delete {0} error {1}", path, ex.Message);
                        }
                        if (!Abstractions.Windows.Security.SetDirOwner(path, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null).Translate(typeof(NTAccount))))
                        {
                            if (x == retry)
                            {
                                m_logger.DebugFormat("failed to set directory owner for {0} at {1}", WindowsIdentity.GetCurrent().Name, path);
                            }
                        }
                        if (!Abstractions.Windows.Security.ReplaceDirectorySecurity(path, new IdentityReference[] { new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null) }, FileSystemRights.FullControl, AccessControlType.Allow, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None))
                        {
                            if (x == retry)
                            {
                                m_logger.DebugFormat("failed to set DirectorySecurity for {0} at {1}", WindowsIdentity.GetCurrent().Name, path);
                            }
                        }
                        else
                        {
                            if (!Abstractions.Windows.Security.SetRecDirAttrib(new DirectoryInfo(path), FileAttributes.Normal))
                            {
                                if (x == retry)
                                {
                                    m_logger.DebugFormat("failed to set Attributes at {0}", path);
                                }
                            }
                        }
                    }
                    if (x == retry)
                    {
                        try
                        {//do better
                            Process.Start("cmd", "/c rd /S /Q \"" + path + "\"");
                        }
                        catch (Exception ex)
                        {
                            m_logger.InfoFormat("failed to run rd /s /q \"{0}\":{1}", path, ex.Message);
                        }
                    }
                }
                else
                {
                    return true;
                }
                Thread.Sleep(new TimeSpan(0, 0, 3));
            }

            return false;
        }

        private static Boolean SetACL(string dir, string username, string password, uint maxstore, uint retry)
        {
            Abstractions.WindowsApi.pInvokes.structenums.USER_INFO_4 userinfo4 = new Abstractions.WindowsApi.pInvokes.structenums.USER_INFO_4();
            if (!Abstractions.WindowsApi.pInvokes.UserGet(username, ref userinfo4))
            {
                m_logger.DebugFormat("Can't get userinfo for user {0}", username);
                return false;
            }

            IdentityReference userIref = new SecurityIdentifier(userinfo4.user_sid).Translate(typeof(NTAccount));
            if (!Abstractions.Windows.Security.SetDirOwner(dir, new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Translate(typeof(NTAccount))))
            {
                m_logger.WarnFormat("Can't set owner for Directory {0}", dir);
                return false;
            }

            if (!Abstractions.Windows.Security.ReplaceDirectorySecurity(dir, new IdentityReference[] { new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), userIref }, FileSystemRights.FullControl, AccessControlType.Allow, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None))
            {
                m_logger.WarnFormat("Can't set ACL for Directory {0}", dir);
                return false;
            }

            if (!Abstractions.Windows.Security.RemoveAccRuleFromUnknownUser(dir))
            {
                m_logger.InfoFormat("Can't remove unknown users from {0} ACL", dir);
                //not critical
            }

            bool work = false;
            for (int x = 1; x <= retry; x++)
            {
                if (!Abstractions.WindowsApi.pInvokes.RegistryLoad(Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation.HKEY_LOCAL_MACHINE, username, dir + "\\NTUSER.DAT"))
                {
                    m_logger.WarnFormat("Can't load regfile {0}\\{1}", dir, "NTUSER.DAT");
                    Thread.Sleep(new TimeSpan(0, 0, 10));
                    work = false;
                    continue;
                }
                else
                {
                    work = true;
                }
                Thread.Sleep(new TimeSpan(0, 0, 3));

                break;
            }
            if (!work)
            {
                return false;
            }

            if (!Abstractions.Windows.User.SetQuota(Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation.HKEY_LOCAL_MACHINE, username, maxstore))
            {
                m_logger.WarnFormat("Can't set quota");
                //not critical
            }

            if (!Abstractions.Windows.Security.RegSec(Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation.HKEY_LOCAL_MACHINE, username))
            {
                m_logger.WarnFormat("Can't set ACL for regkey {0}\\{1}", Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation.HKEY_LOCAL_MACHINE.ToString(), username);
                return false;
            }

            for (int x = 1; x <= retry; x++)
            {
                if (!Abstractions.WindowsApi.pInvokes.RegistryUnLoad(Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation.HKEY_LOCAL_MACHINE, username))
                {
                    m_logger.WarnFormat("Can't unload regkey {0}\\{1}", dir, "NTUSER.DAT");
                    Thread.Sleep(new TimeSpan(0, 0, 10));
                    work = false;
                    continue;
                }
                else
                {
                    work = true;
                }
                Thread.Sleep(new TimeSpan(0, 0, 3));

                break;
            }
            if (!work)
            {
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
            catch
            {
                m_logger.DebugFormat("can't split servername {0}", share);
                return ret;
            }

            if (!String.IsNullOrEmpty(server[0]))
            {
                ret[0] = server[0];
                ret[1] = server[0];
                if (!visversa)
                {
                    return ret;
                }
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
                catch (Exception ex)
                {
                    m_logger.ErrorFormat("can't resolve FQDN of {0}:{1}", server[0], ex.Message);
                    return new string[] { null, null };
                }
            }
            else
            {
                m_logger.DebugFormat("first token of servername {0} is null", share);
            }

            return ret;
        }

        private static Boolean Connect2share(string share, string username, string password, uint retry, Boolean DISconnect)
        {
            if (DISconnect)
            {
                m_logger.DebugFormat("disconnecting from {0}", share);
                if (!Abstractions.WindowsApi.pInvokes.DisconnectNetworkDrive(share))
                {
                    m_logger.WarnFormat("unable to disconnect from {0}", share);
                }

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

                string dusername = server[0] + "\\" + username;

                for (int x = 1; x <= retry; x++)
                {
                    try
                    {
                        m_logger.DebugFormat("{0}. try to connect to {1} as {2}", x, share, dusername);
                        if (!Abstractions.WindowsApi.pInvokes.MapNetworkDrive(share, dusername, password))
                        {
                            m_logger.ErrorFormat("Failed to connect to share {0}", share);
                            Thread.Sleep(new TimeSpan(0, 1, 0));
                            continue;
                        }
                        if (Directory.Exists(share))
                            return true;
                    }
                    catch (Exception ex)
                    {
                        m_logger.Error(ex.Message);
                    }
                    Thread.Sleep(new TimeSpan(0, 0, 3));
                }

                return false;
            }
        }

        private static Boolean CreateRoamingFolder(string dir, string username)
        {
            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch
                {
                    m_logger.WarnFormat("Can't create Directory {0}", dir);
                    return false;
                }
            }

            Abstractions.WindowsApi.pInvokes.structenums.USER_INFO_4 userinfo4 = new Abstractions.WindowsApi.pInvokes.structenums.USER_INFO_4();
            if (!Abstractions.WindowsApi.pInvokes.UserGet(username, ref userinfo4))
            {
                m_logger.WarnFormat("Can't get userinfo4 from {0}", username);
                return false;
            }

            IdentityReference userIref = new SecurityIdentifier(userinfo4.user_sid).Translate(typeof(NTAccount));
            if (!Abstractions.Windows.Security.SetDirOwner(dir, new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Translate(typeof(NTAccount))))
            {
                m_logger.WarnFormat("Can't set owner {0} for Directory {1}", username, dir);
                return false;
            }

            if (!Abstractions.Windows.Security.SetDirectorySecurity(dir, new IdentityReference[] { userIref }, FileSystemRights.Write | FileSystemRights.ReadAndExecute, AccessControlType.Allow, InheritanceFlags.None, PropagationFlags.None))
            {
                m_logger.WarnFormat("Can't set ACL for Directory {0}", dir);
                return false;
            }

            return true;
        }

        private static int RunWait(string application, string arguments, out string stdmerge)
        {
            stdmerge = "";
            string stdout = "";
            string stderr = "";
            int ret = -1;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = application;
            startInfo.Arguments = arguments;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            try
            {
                using (Process p = Process.Start(startInfo))
                {
                    using (StreamReader streamReader = p.StandardOutput)
                    {
                        stdout = streamReader.ReadToEnd();
                    }
                    using (StreamReader streamReader = p.StandardError)
                    {
                        stderr = streamReader.ReadToEnd();
                    }
                    p.WaitForExit();
                    ret = p.ExitCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RunWait failed error:{0}", ex.Message);
                return -1;
            }

            stdmerge += stdout + "\r\n" + stderr;

            return ret;
        }

        private static string tail(string input, int lines)
        {
            string ret = "";

            string t = input.Replace("\r", "");
            while (t.Contains("\n\n"))
            {
                t = t.Replace("\n\n", "\n");
            }
            string[] s = t.Split('\n');
            if (s.Length >= lines)
            {
                ret = "";
                for (int x = s.Length - lines; x < s.Length; x++)
                {
                    ret += s[x] + "\r\n";
                }
            }
            ret.TrimEnd(new char[] { '\r', '\n', ' ', '\t' });

            return ret;
        }
    }
}
