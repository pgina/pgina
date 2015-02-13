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
using System.Diagnostics;
using System.Threading;
using System.Security.Principal;

using log4net;
using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using Abstractions.WindowsApi;
using Abstractions.Windows;

namespace pGina.Plugin.pgSMB
{
    public class PluginImpl : IPluginAuthenticationGateway, IPluginConfiguration, IPluginEventNotifications, IPluginLogoffRequestAddTime
    {
        private ILog m_logger = LogManager.GetLogger("pgSMB");

        private Dictionary<string, Boolean> RunningTasks = new Dictionary<string, Boolean>();
        private ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        public static Boolean IsShuttingDown = false;

        #region Init-plugin
        public static Guid PluginUuid
        {
            get { return new Guid("{616B4DA4-23F9-48C7-8DB4-46E9A6F6BD31}"); }
        }

        public PluginImpl()
        {
            using (Process me = Process.GetCurrentProcess())
            {
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }

        public string Name
        {
            get { return "pgSMB"; }
        }

        public string Description
        {
            get { return "a \"clone\" of the pGina 1.x pgFtp plugin"; }
        }

        public Guid Uuid
        {
            get { return PluginUuid; }
        }

        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        #endregion

        public void Starting() { }
        public void Stopping() { }

        public Boolean LogoffRequestAddTime()
        {
            IsShuttingDown = true;
            try
            {
                Locker.TryEnterReadLock(-1);
                if (RunningTasks.Values.Contains(true))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                m_logger.InfoFormat("LogoffRequestAddTime() error {0}", ex.Message);
            }
            finally
            {
                Locker.ExitReadLock();
            }

            return false;
        }

        public Boolean LoginUserRequest(string username)
        {
            try
            {
                Locker.TryEnterReadLock(-1);
                if (RunningTasks.Keys.Contains(username.ToLower()))
                {
                    m_logger.InfoFormat("LoginUserRequest() logoff in process for {0}", username);
                    return true;
                }
                else
                {
                    m_logger.InfoFormat("LoginUserRequest() {0} free to login", username);
                    return false;
                }
            }
            catch (Exception ex)
            {
                m_logger.InfoFormat("LoginUserRequest() {0} error {1}", username, ex.Message);
            }
            finally
            {
                Locker.ExitReadLock();
            }

            return false;
        }

        private Dictionary<string, string> GetSettings(string username, UserInformation userInfo)
        {
            Dictionary<string,string> settings = new Dictionary<string,string>();
            Abstractions.WindowsApi.pInvokes.structenums.OSVERSIONINFOW verinfo = Abstractions.WindowsApi.pInvokes.VersionsInfo();
            if (verinfo.dwMajorVersion == 0)
            {
                m_logger.WarnFormat("GetSettings: VersionsInfo() failed. I'm unable to detect OS beyond Windows 8.0");
                verinfo.dwBuildNumber = Environment.OSVersion.Version.Build;
                verinfo.dwMajorVersion = Environment.OSVersion.Version.Major;
                verinfo.dwMinorVersion = Environment.OSVersion.Version.Minor;
                verinfo.dwPlatformId = Environment.OSVersion.Version.Build;
            }

            string[,] array = new string[,]
            {
                {"RoamingDest", Settings.Store.RoamingDest.ToString()},
                {"RoamingDest_real", (Environment.OSVersion.Version.Major == 6) ? (verinfo.dwMinorVersion > 3)/*greater than 8.1*/ ? Settings.Store.RoamingDest.ToString()+".V5" : Settings.Store.RoamingDest.ToString()+".V2" : Settings.Store.RoamingDest.ToString() },
                {"Filename", (!String.IsNullOrEmpty(userInfo.pgSMB_Filename)) ? userInfo.pgSMB_Filename : Settings.Store.Filename.ToString()},
                {"SMBshare", (!String.IsNullOrEmpty(userInfo.pgSMB_SMBshare)) ? userInfo.pgSMB_SMBshare : Settings.Store.SMBshare.ToString()},
                {"RoamingSource", (!String.IsNullOrEmpty(userInfo.usri4_profile)) ? userInfo.usri4_profile : Settings.Store.RoamingSource.ToString()},
                {"ConnectRetry", Settings.Store.ConnectRetry.ToString()},
                {"Compressor", Settings.Store.Compressor.ToString()},
                {"UncompressCLI", Settings.Store.UncompressCLI.ToString()},
                {"CompressCLI", Settings.Store.CompressCLI.ToString()},
                {"MaxStore", (userInfo.usri4_max_storage > 0) ? userInfo.usri4_max_storage.ToString() : Settings.Store.MaxStore.ToString()},
                {"ntp", Settings.Store.ntp.ToString()},
                /*maybe emty*/
                {"HomeDir", (!String.IsNullOrEmpty(userInfo.usri4_home_dir)) ? userInfo.usri4_home_dir : Settings.Store.HomeDir.ToString()},
                {"HomeDirDrive", (!String.IsNullOrEmpty(userInfo.usri4_home_dir_drive)) ? userInfo.usri4_home_dir_drive : Settings.Store.HomeDirDrive.ToString()},
                {"ScriptPath", (!String.IsNullOrEmpty(userInfo.LoginScript)) ? userInfo.LoginScript : Settings.Store.ScriptPath.ToString()},
                {"email", Settings.Store.email.ToString()},
                {"smtp", Settings.Store.smtp.ToString()}
            };
            for (uint x = 0; x <= array.GetUpperBound(0); x++ )
            {
                array[x, 1] = Environment.ExpandEnvironmentVariables(array[x, 1]).Replace("%u", username);
                if (x > 2)
                {
                    // mask the filename in the string to prevent a replacement
                    array[x, 1] = array[x, 1].Replace(array[2, 1],"??????????");
                    // replace the roaming folder (NT6 adds .V2)
                    array[x, 1] = array[x, 1].Replace(array[0, 1], array[1, 1]);
                    // unmask the filename
                    array[x, 1] = array[x, 1].Replace("??????????", array[2, 1]);
                }
                if (x < 11) // first 11 fields shouldnt be empty
                {
                    if (String.IsNullOrEmpty(array[x, 1]))
                    {
                        settings.Add("ERROR", array[x, 0]);
                    }
                }
                settings.Add(array[x, 0], array[x, 1]);
            }

            List<string> keys = new List<string>(settings.Keys);
            for (uint y = 1; y <= 4; y++)
            {
                foreach (string key in keys)
                {
                    settings[key] = settings[key].Replace("%f", settings["Filename"]);
                    settings[key] = settings[key].Replace("%s", settings["SMBshare"]);
                    settings[key] = settings[key].Replace("%r", settings["RoamingSource"]);
                    settings[key] = settings[key].Replace("%d", settings["RoamingDest_real"]);
                }
            }

            foreach (KeyValuePair<string, string> pair in settings)
            {
                m_logger.DebugFormat("GetSettings:\"{0}\" \"{1}\"",pair.Key,pair.Value);
            }

            return settings;
        }

        public BooleanResult AuthenticatedUserGateway(SessionProperties properties)
        {
            // get user info
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            BooleanResult RetBool = new BooleanResult();

            // get the plugin settings
            Dictionary<string,string> settings = GetSettings(userInfo.Username, userInfo);
            if (settings.ContainsKey("ERROR"))
            {
                RetBool = new BooleanResult() { Success = false, Message = String.Format("Can't parse plugin settings ", settings["ERROR"]) };
                pGina.Core.Settings.sendMail(userInfo.Username, userInfo.Password, String.Format("pGina: unable to Login {0} from {1}", userInfo.Username, Environment.MachineName), RetBool.Message);
                return RetBool;
            }

            RetBool = Roaming.get(settings, userInfo.Username, userInfo.Password);
            if (!RetBool.Success)
            {
                //Roaming.email(settings["email"], settings["smtp"], userInfo.Username, userInfo.Password, String.Format("pGina: unable to Login {0} from {1}", userInfo.Username, Environment.MachineName), RetBool.Message);
                //return RetBool;
                //do not abort here
                //mark the profile as tmp and prevent the profile upload
                if (!Roaming.userAdd(settings, userInfo.Username, userInfo.Password, "pGina created pgSMB tmp"))
                {
                    Roaming.userDel(settings, userInfo.Username, userInfo.Password);
                    pGina.Core.Settings.sendMail(userInfo.Username, userInfo.Password, String.Format("pGina: tmp Login failed {0} from {1}", userInfo.Username, Environment.MachineName), "tmp login failed");
                    return RetBool;
                }
                pGina.Core.Settings.sendMail(userInfo.Username, userInfo.Password, String.Format("pGina: tmp Login {0} from {1}", userInfo.Username, Environment.MachineName), "failed to get the profile\nmarking as tmp");
            }

            pInvokes.structenums.USER_INFO_4 userinfo4 = new pInvokes.structenums.USER_INFO_4();
            if (pInvokes.UserGet(userInfo.Username, ref userinfo4))
            {
                if (RetBool.Success)
                {
                    userInfo.SID = new SecurityIdentifier(userinfo4.user_sid);
                }
                userInfo.Description = userinfo4.comment;
            }
            else // we should never go there
            {
                if (RetBool.Success)
                {
                    userInfo.Description = "pGina created pgSMB";
                }
                else
                {
                    userInfo.Description = "pGina created pgSMB tmp";
                }
            }
            properties.AddTrackedSingle<UserInformation>(userInfo);

            return new BooleanResult() { Success = true };
            //return new BooleanResult() { Success = false, Message = "Incorrect username or password." };
        }

        public void Configure()
        {
            Configuration dialog = new Configuration();
            dialog.ShowDialog();
        }

        public void SessionChange(System.ServiceProcess.SessionChangeDescription changeDescription, SessionProperties properties)
        {
            if (properties == null || changeDescription == null)
            {
                return;
            }

            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            if (!userInfo.HasSID)
            {
                m_logger.InfoFormat("SessionChange Event denied for ID:{0}", changeDescription.SessionId);
                return;
            }

            if (changeDescription.Reason == System.ServiceProcess.SessionChangeReason.SessionLogoff)
            {
                m_logger.DebugFormat("SessionChange SessionLogoff for ID:{0} as user:{1}", changeDescription.SessionId, userInfo.Username);

                if (userInfo.Description.Contains("pGina created pgSMB"))
                {
                    try
                    {
                        Locker.TryEnterWriteLock(-1);
                        RunningTasks.Add(userInfo.Username.ToLower(), true);
                    }
                    finally
                    {
                        Locker.ExitWriteLock();
                    }

                    Thread rem_smb = new Thread(() => cleanup(userInfo, changeDescription.SessionId));
                    rem_smb.Start();
                }
                else
                {
                    m_logger.InfoFormat("User {0} is'nt a pGina pgSMB plugin created user. I'm not executing Notification stage", userInfo.Username);
                }
            }
            if (changeDescription.Reason == System.ServiceProcess.SessionChangeReason.SessionLogon)
            {
                m_logger.DebugFormat("SessionChange SessionLogon for ID:{0} as user:{1}", changeDescription.SessionId, userInfo.Username);

                if (userInfo.Description.Contains("pGina created pgSMB"))
                {
                    Dictionary<string, string> settings = GetSettings(userInfo.Username, userInfo);

                    if (!String.IsNullOrEmpty(settings["ScriptPath"]))
                    {
                        try
                        {
                            Abstractions.WindowsApi.pInvokes.StartUserProcessInSession(changeDescription.SessionId, settings["ScriptPath"]);
                        }
                        catch (Exception ex)
                        {
                            m_logger.ErrorFormat("Can't run application {0} because {1}", settings["ScriptPath"], ex.ToString());
                            pInvokes.SendMessageToUser(changeDescription.SessionId, "Can't run application", String.Format("I'm unable to run your LoginScript\n{0}", settings["ScriptPath"]));
                        }
                    }

                    if (!Abstractions.Windows.User.QueryQuota(pInvokes.structenums.RegistryLocation.HKEY_USERS, userInfo.SID.ToString()) && Convert.ToUInt32(settings["MaxStore"]) > 0)
                    {
                        m_logger.InfoFormat("no quota GPO settings for user {0}", userInfo.SID.ToString());
                        if (!Abstractions.Windows.User.SetQuota(pInvokes.structenums.RegistryLocation.HKEY_USERS, userInfo.SID.ToString(), Convert.ToUInt32(settings["MaxStore"])))
                        {
                            m_logger.InfoFormat("failed to set quota GPO for user {0}", userInfo.SID.ToString());
                        }
                        else
                        {
                            m_logger.InfoFormat("done quota GPO settings for user {0}", userInfo.SID.ToString());
                            try
                            {
                                Abstractions.WindowsApi.pInvokes.StartUserProcessInSession(changeDescription.SessionId, "proquota.exe");
                            }
                            catch (Exception ex)
                            {
                                m_logger.ErrorFormat("Can't run application {0} because {1}", "proquota.exe", ex.ToString());
                            }
                        }
                    }

                    m_logger.DebugFormat("removing Roaming Profile {0}", settings["RoamingDest_real"]);
                    if (!Roaming.DirectoryDel(settings["RoamingDest_real"], Convert.ToUInt32(settings["ConnectRetry"])))
                    {
                        m_logger.WarnFormat("Can't delete {0}", settings["RoamingDest_real"]);
                    }

                    IntPtr hToken = Abstractions.WindowsApi.pInvokes.GetUserToken(userInfo.Username, null, userInfo.Password);
                    if (hToken != IntPtr.Zero)
                    {
                        string uprofile = Abstractions.WindowsApi.pInvokes.GetUserProfilePath(hToken);
                        if (String.IsNullOrEmpty(uprofile))
                        {
                            uprofile = Abstractions.WindowsApi.pInvokes.GetUserProfileDir(hToken);
                        }
                        Abstractions.WindowsApi.pInvokes.CloseHandle(hToken);

                        if (uprofile.Contains(@"\TEMP"))
                        {
                            pGina.Core.Settings.sendMail(userInfo.Username, userInfo.Password, String.Format("pGina: Windows tmp Login {0} from {1}", userInfo.Username, Environment.MachineName), "Windows was unable to load the profile");
                        }

                        Abstractions.WindowsApi.pInvokes.CloseHandle(hToken);
                    }
                }
                else
                {
                    m_logger.InfoFormat("User {0} is'nt a pGina pgSMB plugin created user. I'm not executing Notification stage", userInfo.Username);
                }
            }
        }

        private void cleanup(UserInformation userInfo, int sessionID)
        {
            Dictionary<string, string> settings = GetSettings(userInfo.Username, userInfo);

            try
            {
                while (true)
                {
                    // logoff detection is quite a problem under NT6
                    // a disconnectEvent is only triggered during a logoff
                    // but not during a shutdown/reboot
                    // and the SessionLogoffEvent is only saying that the user is logging of
                    // So, there is no event that is fired during a user-logoff/reboot/shutdown
                    // that indicates that the user has logged of
                    if (Abstractions.WindowsApi.pInvokes.IsSessionLoggedOFF(sessionID) || IsShuttingDown)
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }

                if (userInfo.Description.Contains(" tmp")) //its a tmp profile do not upload
                {
                    m_logger.InfoFormat("User {0} has tmp included in his comment \"{1}\" and so the profile is not uploaded", userInfo.Username, userInfo.Description);
                    if (!Roaming.DirectoryDel(settings["RoamingDest_real"], Convert.ToUInt32(settings["ConnectRetry"])))
                    {
                        m_logger.WarnFormat("Can't delete {0}", settings["RoamingDest_real"]);
                    }
                }
                else // upload the profile
                {
                    BooleanResult RetBool = Roaming.put(settings, userInfo.Username, userInfo.Password);
                    if (!RetBool.Success)
                    {
                        pGina.Core.Settings.sendMail(userInfo.Username, userInfo.Password, String.Format("pGina: unable to Logoff {0} from {1}", userInfo.Username, Environment.MachineName), RetBool.Message);
                    }
                }
                m_logger.Info("cleanup done");
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("Error during Logoff", ex.Message);
                pGina.Core.Settings.sendMail(userInfo.Username, userInfo.Password, String.Format("pGina: Logoff Exception {0} from {1}", userInfo.Username, Environment.MachineName), "Logoff Exception\n" + ex.Message);
            }

            try
            {
                Locker.TryEnterWriteLock(-1);
                RunningTasks.Remove(userInfo.Username.ToLower());
            }
            finally
            {
                Locker.ExitWriteLock();
            }
        }
    }
}
