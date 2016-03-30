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
using System.Diagnostics;
using System.Threading;
using System.Security.Principal;

using log4net;
using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using Abstractions.WindowsApi;
using Abstractions.Windows;

namespace pGina.Plugin.pgSMB2
{
    public class PluginImpl : IPluginAuthenticationGateway, IPluginConfiguration, IPluginEventNotifications, IPluginLogoffRequestAddTime
    {
        private ILog m_logger = LogManager.GetLogger("pgSMB2");

        private Dictionary<string, Boolean> RunningTasks = new Dictionary<string, Boolean>();
        private ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        public static Boolean IsShuttingDown = false;

        #region Init-plugin
        public static Guid PluginUuid
        {
            get { return new Guid("{10C41590-07F9-4FF2-B77B-E51E7A51206F}"); }
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
            get { return "pgSMB2"; }
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
            Dictionary<string,string> global_settings = pGina.Shared.Settings.pGinaDynamicSettings.GetSettings(pGina.Shared.Settings.pGinaDynamicSettings.pGinaRoot, new string[] { "" });

            string[,] array = new string[,]
            {
                {"TempComp", Settings.Store.TempComp.ToString()},
                {"Filename", (!String.IsNullOrEmpty(userInfo.pgSMB_Filename)) ? userInfo.pgSMB_Filename : Settings.Store.Filename.ToString()},
                {"SMBshare", (!String.IsNullOrEmpty(userInfo.pgSMB_SMBshare)) ? userInfo.pgSMB_SMBshare : Settings.Store.SMBshare.ToString()},
                {"RoamingSource", (!String.IsNullOrEmpty(userInfo.usri4_profile)) ? userInfo.usri4_profile : Settings.Store.RoamingSource.ToString()},
                {"ConnectRetry", Settings.Store.ConnectRetry.ToString()},
                {"Compressor", Settings.Store.Compressor.ToString()},
                {"UncompressCLI", Settings.Store.UncompressCLI.ToString()},
                {"CompressCLI", Settings.Store.CompressCLI.ToString()},
                {"MaxStore", (userInfo.usri4_max_storage > 0) ? userInfo.usri4_max_storage.ToString() : Settings.Store.MaxStore.ToString()},
                {"ntp", (global_settings.ContainsKey("ntpservers")? global_settings["ntpservers"].Replace('\0',' ') : "")},
                /*maybe emty*/
                {"MaxStoreExclude", Settings.StoreGlobal.MaxStoreExclude.ToString()},
                {"HomeDir", (!String.IsNullOrEmpty(userInfo.usri4_home_dir)) ? userInfo.usri4_home_dir : Settings.Store.HomeDir.ToString()},
                {"HomeDirDrive", (!String.IsNullOrEmpty(userInfo.usri4_home_dir_drive)) ? userInfo.usri4_home_dir_drive : Settings.Store.HomeDirDrive.ToString()},
                {"ScriptPath", (!String.IsNullOrEmpty(userInfo.LoginScript)) ? userInfo.LoginScript : Settings.Store.ScriptPath.ToString()},
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
                    settings[key] = settings[key].Replace("%d", settings["TempComp"]);
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
                Abstractions.Windows.Networking.sendMail(pGina.Shared.Settings.pGinaDynamicSettings.GetSettings(pGina.Shared.Settings.pGinaDynamicSettings.pGinaRoot, new string[] { "notify_pass" }), userInfo.Username, userInfo.Password, String.Format("pGina: unable to Login {0} from {1}", userInfo.Username, Environment.MachineName), RetBool.Message);
                return RetBool;
            }

            Roaming ro = new Roaming();
            RetBool = ro.get(settings, userInfo.Username, userInfo.Password);
            if (!RetBool.Success)
            {
                //Roaming.email(settings["email"], settings["smtp"], userInfo.Username, userInfo.Password, String.Format("pGina: unable to Login {0} from {1}", userInfo.Username, Environment.MachineName), RetBool.Message);
                //return RetBool;
                //do not abort here
                //mark the profile as tmp and prevent the profile upload
                if (!ro.userAdd(settings, userInfo.Username, userInfo.Password, "pGina created pgSMB2 tmp"))
                {
                    ro.userDel(settings, userInfo.Username, userInfo.Password);
                    Abstractions.Windows.Networking.sendMail(pGina.Shared.Settings.pGinaDynamicSettings.GetSettings(pGina.Shared.Settings.pGinaDynamicSettings.pGinaRoot, new string[] { "notify_pass" }), userInfo.Username, userInfo.Password, String.Format("pGina: tmp Login failed {0} from {1}", userInfo.Username, Environment.MachineName), "tmp login failed");
                    return RetBool;
                }
                Abstractions.Windows.Networking.sendMail(pGina.Shared.Settings.pGinaDynamicSettings.GetSettings(pGina.Shared.Settings.pGinaDynamicSettings.pGinaRoot, new string[] { "notify_pass" }), userInfo.Username, userInfo.Password, String.Format("pGina: tmp Login {0} from {1}", userInfo.Username, Environment.MachineName), "failed to get the profile\nmarking as tmp");
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
                    userInfo.Description = "pGina created pgSMB2";
                }
                else
                {
                    userInfo.Description = "pGina created pgSMB2 tmp";
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

        public void SessionChange(int SessionId, System.ServiceProcess.SessionChangeReason Reason, SessionProperties properties)
        {
            if (properties == null)
            {
                return;
            }

            if (Reason == System.ServiceProcess.SessionChangeReason.SessionLogoff)
            {
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
                m_logger.DebugFormat("{1} SessionChange SessionLogoff for ID:{0}", SessionId, userInfo.Username);
                m_logger.InfoFormat("{3} {0} {1} {2}", userInfo.Description.Contains("pGina created"), userInfo.HasSID, properties.CREDUI, userInfo.Username);

                if (userInfo.Description.Contains("pGina created") && userInfo.HasSID && !properties.CREDUI)
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

                    // add this plugin into PluginActivityInformation
                    m_logger.DebugFormat("{1} properties.id:{0}", properties.Id, userInfo.Username);

                    PluginActivityInformation notification = properties.GetTrackedSingle<PluginActivityInformation>();
                    foreach (Guid gui in notification.GetNotificationPlugins())
                    {
                        m_logger.DebugFormat("{1} PluginActivityInformation Guid:{0}", gui, userInfo.Username);
                    }
                    m_logger.DebugFormat("{1} PluginActivityInformation add guid:{0}", PluginUuid, userInfo.Username);
                    notification.AddNotificationResult(PluginUuid, new BooleanResult { Message = "", Success = false });
                    properties.AddTrackedSingle<PluginActivityInformation>(notification);
                    foreach (Guid gui in notification.GetNotificationPlugins())
                    {
                        m_logger.DebugFormat("{1} PluginActivityInformation Guid:{0}", gui, userInfo.Username);
                    }

                    Thread rem_smb = new Thread(() => cleanup(userInfo, SessionId, properties));
                    rem_smb.Start();
                }
                else
                {
                    m_logger.InfoFormat("{0} {1}. I'm not executing Notification stage", userInfo.Username, (properties.CREDUI) ? "has a program running in his context" : "is'nt a pGina created user");
                }
            }
            if (Reason == System.ServiceProcess.SessionChangeReason.SessionLogon)
            {
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
                if (!userInfo.HasSID)
                {
                    m_logger.InfoFormat("{1} SessionLogon Event denied for ID:{0}", SessionId, userInfo.Username);
                    return;
                }

                m_logger.DebugFormat("{1} SessionChange SessionLogon for ID:{0}", SessionId, userInfo.Username);

                if (userInfo.Description.Contains("pGina created pgSMB2"))
                {
                    Dictionary<string, string> settings = GetSettings(userInfo.Username, userInfo);

                    if (!String.IsNullOrEmpty(settings["ScriptPath"]))
                    {
                        if (!Abstractions.WindowsApi.pInvokes.StartUserProcessInSession(SessionId, settings["ScriptPath"]))
                        {
                            m_logger.ErrorFormat("Can't run application {0}", settings["ScriptPath"]);
                            Abstractions.WindowsApi.pInvokes.SendMessageToUser(SessionId, "Can't run application", String.Format("I'm unable to run your LoginScript\n{0}", settings["ScriptPath"]));
                        }
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
                        m_logger.InfoFormat("add LocalProfilePath:[{0}]", uprofile);
                        // the profile realy exists there, instead of assuming it will be created or changed during a login (temp profile[win error reading profile])
                        userInfo.LocalProfilePath = uprofile;
                        properties.AddTrackedSingle<UserInformation>(userInfo);

                        if ((uprofile.Contains(@"\TEMP") && !userInfo.Username.StartsWith("temp", StringComparison.CurrentCultureIgnoreCase)) || Abstractions.Windows.User.IsProfileTemp(userInfo.SID.ToString()) == true)
                        {
                            m_logger.InfoFormat("TEMP profile detected");

                            string userInfo_old_Description = userInfo.Description;
                            userInfo.Description = "pGina created pgSMB2 tmp";
                            properties.AddTrackedSingle<UserInformation>(userInfo);

                            pInvokes.structenums.USER_INFO_4 userinfo4 = new pInvokes.structenums.USER_INFO_4();
                            if (pInvokes.UserGet(userInfo.Username, ref userinfo4))
                            {
                                userinfo4.logon_hours = IntPtr.Zero;
                                userinfo4.comment = userInfo.Description;
                                if (!pInvokes.UserMod(userInfo.Username, userinfo4))
                                {
                                    m_logger.ErrorFormat("Can't modify userinformation {0}", userInfo.Username);
                                }
                            }
                            else
                            {
                                m_logger.ErrorFormat("Can't get userinformation {0}", userInfo.Username);
                            }

                            if (userInfo_old_Description.EndsWith("pGina created pgSMB2"))
                            {
                                Abstractions.Windows.Networking.sendMail(pGina.Shared.Settings.pGinaDynamicSettings.GetSettings(pGina.Shared.Settings.pGinaDynamicSettings.pGinaRoot, new string[] { "notify_pass" }), userInfo.Username, userInfo.Password, String.Format("pGina: Windows tmp Login {0} from {1}", userInfo.Username, Environment.MachineName), "Windows was unable to load the profile");
                            }
                        }
                    }


                    if (userInfo.Description.EndsWith("pGina created pgSMB2"))
                    {

                        try
                        {
                            if (!EventLog.SourceExists("proquota"))
                                EventLog.CreateEventSource("proquota", "Application");
                        }
                        catch
                        {
                            EventLog.CreateEventSource("proquota", "Application");
                        }
                        Abstractions.Windows.User.SetQuota(pInvokes.structenums.RegistryLocation.HKEY_USERS, userInfo.SID.ToString(), 0);

                        string proquotaPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "proquota.exe");
                        try
                        {
                            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Exclusions\Processes", true))
                            {
                                if (key != null)
                                {
                                    bool proquota_exclude_found = false;
                                    foreach (string ValueName in key.GetValueNames())
                                    {
                                        if (ValueName.Equals(proquotaPath, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            proquota_exclude_found = true;
                                        }
                                    }

                                    if (!proquota_exclude_found)
                                    {
                                        key.SetValue(proquotaPath, 0, Microsoft.Win32.RegistryValueKind.DWord);
                                    }
                                }
                            }
                        }
                        catch { }

                        m_logger.InfoFormat("start session:{0} prog:{1}", SessionId, proquotaPath);
                        if (!Abstractions.WindowsApi.pInvokes.StartUserProcessInSession(SessionId, proquotaPath + " \"" + userInfo.LocalProfilePath + "\" " + settings["MaxStore"]))
                        {
                            m_logger.ErrorFormat("{0} Can't run application {1}", userInfo.Username, "proquota.exe");
                        }
                    }
                }
                else
                {
                    m_logger.InfoFormat("{0} is'nt a pGina pgSMB2 plugin created user. I'm not executing Notification stage", userInfo.Username);
                }
            }
        }

        private void cleanup(UserInformation userInfo, int sessionID, SessionProperties properties)
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
                while (true)
                {
                    // if no other notification plugin is working on this user
                    // if the first entry from GetNotificationPlugins is equal to this plugin UID
                    IEnumerable<Guid> guids = properties.GetTrackedSingle<PluginActivityInformation>().GetNotificationPlugins();
                    /*foreach(Guid gui in guids)
                    {
                        m_logger.DebugFormat("{1} PluginActivityInformation guid:{0}", gui, userInfo.Username);
                    }*/
                    if (guids.DefaultIfEmpty(Guid.Empty).FirstOrDefault().Equals(PluginUuid) || guids.ToList().Count == 0)
                    {
                        break;
                    }

                    Thread.Sleep(1000);
                }

                Roaming ro = new Roaming();
                if (!userInfo.Description.EndsWith(" tmp")) //its a tmp profile do not upload
                {
                    BooleanResult RetBool = ro.put(settings, userInfo.Username, userInfo.Password);
                    if (!RetBool.Success)
                    {
                        Abstractions.Windows.Networking.sendMail(pGina.Shared.Settings.pGinaDynamicSettings.GetSettings(pGina.Shared.Settings.pGinaDynamicSettings.pGinaRoot, new string[] { "notify_pass" }), userInfo.Username, userInfo.Password, String.Format("pGina: unable to Logoff {0} from {1}", userInfo.Username, Environment.MachineName), RetBool.Message);
                    }
                }
                m_logger.InfoFormat("{0} cleanup done", userInfo.Username);
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("{0} Error during Logoff of {1}", userInfo.Username, ex.Message);
                Abstractions.Windows.Networking.sendMail(pGina.Shared.Settings.pGinaDynamicSettings.GetSettings(pGina.Shared.Settings.pGinaDynamicSettings.pGinaRoot, new string[] { "notify_pass" }), userInfo.Username, userInfo.Password, String.Format("pGina: Logoff Exception {0} from {1}", userInfo.Username, Environment.MachineName), "Logoff Exception\n" + ex.Message);
            }

            try
            {
                Locker.TryEnterWriteLock(-1);
                RunningTasks.Remove(userInfo.Username.ToLower());

                PluginActivityInformation notification = properties.GetTrackedSingle<PluginActivityInformation>();
                notification.DelNotificationResult(PluginUuid);
                m_logger.InfoFormat("{1} PluginActivityInformation del Guid:{0}", PluginUuid, userInfo.Username);
                properties.AddTrackedSingle<PluginActivityInformation>(notification);
                foreach (Guid guid in properties.GetTrackedSingle<PluginActivityInformation>().GetNotificationPlugins())
                {
                    m_logger.InfoFormat("{1} PluginActivityInformation Guid:{0}", guid, userInfo.Username);
                }
            }
            finally
            {
                Locker.ExitWriteLock();
            }
        }
    }
}
