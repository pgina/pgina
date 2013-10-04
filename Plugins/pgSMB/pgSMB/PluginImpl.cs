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
using System.Diagnostics;
using System.Threading;

using log4net;
using pGina.Shared.Interfaces;
using pGina.Shared.Types;

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
                    return true;
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

        private Dictionary<string, string> GetSettings(string username)
        {
            Dictionary<string,string> settings = new Dictionary<string,string>();

            string RoamingDest_real = Settings.Store.RoamingDest.ToString();
            if (Environment.OSVersion.Version.Major == 6)
                RoamingDest_real += ".V2";

            string[,] array = new string[,]
            {
                {"RoamingDest", Settings.Store.RoamingDest.ToString()},
                {"RoamingDest_real", RoamingDest_real},
                {"Filename", Settings.Store.Filename.ToString()},
                {"SMBshare", Settings.Store.SMBshare.ToString()},
                {"RoamingSource", Settings.Store.RoamingSource.ToString()},
                {"ConnectRetry", Settings.Store.ConnectRetry.ToString()},
                {"Compressor", Settings.Store.Compressor.ToString()},
                {"UncompressCLI", Settings.Store.UncompressCLI.ToString()},
                {"CompressCLI", Settings.Store.CompressCLI.ToString()},
                {"MaxStore", Settings.Store.MaxStore.ToString()},
                {"ntp", Settings.Store.ntp.ToString()},
                /*maybe emty*/
                {"HomeDir", Settings.Store.HomeDir.ToString()},
                {"HomeDirDrive", Settings.Store.HomeDirDrive.ToString()},
                {"ScriptPath", Settings.Store.ScriptPath.ToString()},
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
                    if (String.IsNullOrEmpty(array[x, 1]))
                        settings.Add("ERROR", array[x, 0]);
                settings.Add(array[x, 0], array[x, 1]);
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
            Dictionary<string,string> settings = GetSettings(userInfo.Username);
            if (settings.ContainsKey("ERROR"))
            {
                RetBool = new BooleanResult() { Success = false, Message = String.Format("Can't parse plugin settings ", settings["ERROR"]) };
                Roaming.email(settings["email"], settings["smtp"], userInfo.Username, userInfo.Password, String.Format("pGina: unable to Login {0} from {1}", userInfo.Username, Environment.MachineName), RetBool.Message);
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
                    Roaming.email(settings["email"], settings["smtp"], userInfo.Username, userInfo.Password, String.Format("pGina: tmp Login failed {0} from {1}", userInfo.Username, Environment.MachineName), "tmp login failed");
                    return RetBool;
                }
                Roaming.email(settings["email"], settings["smtp"], userInfo.Username, userInfo.Password, String.Format("pGina: tmp Login {0} from {1}", userInfo.Username, Environment.MachineName), "failed to get the profile\nmarking as tmp");
            }

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
                return;

            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            try
            {
                String.IsNullOrEmpty(userInfo.Username.Length.ToString());
                String.IsNullOrEmpty(userInfo.Password.Length.ToString());
                String.IsNullOrEmpty(userInfo.Description.Length.ToString());
                String.IsNullOrEmpty(userInfo.SID.ToString().Length.ToString());
            }
            catch
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
            }
            if (changeDescription.Reason == System.ServiceProcess.SessionChangeReason.SessionLogon)
            {
                m_logger.DebugFormat("SessionChange SessionLogon for ID:{0} as user:{1}", changeDescription.SessionId, userInfo.Username);

                if (userInfo.Description.Contains("pGina created pgSMB"))
                {
                    Dictionary<string, string> settings = GetSettings(userInfo.Username);

                    if (!String.IsNullOrEmpty(settings["ScriptPath"]))
                    {
                        try
                        {
                            Abstractions.WindowsApi.pInvokes.StartUserProcessInSession(changeDescription.SessionId, settings["ScriptPath"]);
                        }
                        catch (Exception ex)
                        {
                            m_logger.ErrorFormat("Can't run application {0} because {1}", settings["ScriptPath"], ex.ToString());
                        }
                    }

                    if (!registry.RegQueryQuota(RegistryLocation.HKEY_USERS, userInfo.SID.ToString()) && Convert.ToUInt32(settings["MaxStore"]) > 0)
                    {
                        m_logger.InfoFormat("no quota GPO settings for user {0}", userInfo.SID.ToString());
                        if (!Roaming.SetGPO(RegistryLocation.HKEY_USERS, Convert.ToUInt32(settings["MaxStore"]), userInfo.SID.ToString()))
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
                        m_logger.WarnFormat("Can't delete {0}", settings["RoamingDest_real"]);
                }
            }
        }

        private void cleanup(UserInformation userInfo, int sessionID)
        {
            Dictionary<string, string> settings = GetSettings(userInfo.Username);

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
                        m_logger.WarnFormat("Can't delete {0}", settings["RoamingDest_real"]);
                }
                else // upload the profile
                {
                    BooleanResult RetBool = Roaming.put(settings, userInfo.Username, userInfo.Password);
                    if (!RetBool.Success)
                        Roaming.email(settings["email"], settings["smtp"], userInfo.Username, userInfo.Password, String.Format("pGina: unable to Logoff {0} from {1}", userInfo.Username, Environment.MachineName), RetBool.Message);
                }
                m_logger.Info("cleanup done");
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("Error during Logoff", ex.Message);
                Roaming.email(settings["email"], settings["smtp"], userInfo.Username, userInfo.Password, String.Format("pGina: Logoff Exception {0} from {1}", userInfo.Username, Environment.MachineName), "Logoff Exception\n"+ex.Message);
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
