using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using pGina.Shared.Types;
using log4net;
using System.Text.RegularExpressions;
using System.Threading;

namespace pGina.Plugin.scripting
{
    public class PluginImpl : pGina.Shared.Interfaces.IPluginAuthentication, pGina.Shared.Interfaces.IPluginAuthorization, pGina.Shared.Interfaces.IPluginAuthenticationGateway, pGina.Shared.Interfaces.IPluginChangePassword, pGina.Shared.Interfaces.IPluginEventNotifications, pGina.Shared.Interfaces.IPluginLogoffRequestAddTime, pGina.Shared.Interfaces.IPluginConfiguration
    {
        internal class notify : System.Collections.IEnumerable
        {
            internal bool pwd { get; set; }
            internal bool logon { get; set; }
            internal bool logoff { get; set; }
            internal string script { get; set; }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                yield break;
            }
        }

        private ILog m_logger = LogManager.GetLogger("scripting");

        private Dictionary<string, Boolean> RunningTasks = new Dictionary<string, Boolean>();
        private ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        public static Boolean IsShuttingDown = false;

        #region Init-plugin
        public static Guid PluginUuid
        {
            get { return new Guid("{EA94CEBF-6ED1-454A-B333-81C4FAD61FA4}"); }
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
            get { return "scripting"; }
        }

        public string Description
        {
            get { return "run scripts in various stages"; }
        }

        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public Guid Uuid
        {
            get { return PluginUuid; }
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

        public void Configure()
        {
            Configuration myDialog = new Configuration();
            myDialog.ShowDialog();
        }

        internal Dictionary<string, List<notify>> GetSettings(UserInformation userInfo)
        {
            Dictionary<string, List<notify>> ret = new Dictionary<string, List<notify>>();
            //Dictionary<bool, string> lines = new Dictionary<bool, string>();
            List<notify> lines = new List<notify>();

            if (ParseSettings((string[])Settings.Store.authe_sys, ref lines))
                ret.Add("authe_sys", lines);
            if (!String.IsNullOrEmpty(userInfo.script_authe_sys))
                if (ParseSettings((string[])Settings.Store.authe_sys, ref lines))
                    ret.Add("authe_sys", lines);

            if (ParseSettings((string[])Settings.Store.autho_sys, ref lines))
                ret.Add("autho_sys", lines);
            if (!String.IsNullOrEmpty(userInfo.script_autho_sys))
                if (ParseSettings((string[])Settings.Store.autho_sys, ref lines))
                    ret.Add("autho_sys", lines);

            if (ParseSettings((string[])Settings.Store.gateway_sys, ref lines))
                ret.Add("gateway_sys", lines);
            if (!String.IsNullOrEmpty(userInfo.script_gateway_sys))
                if (ParseSettings((string[])Settings.Store.gateway_sys, ref lines))
                    ret.Add("gateway_sys", lines);

            if (ParseSettings((string[])Settings.Store.notification_sys, ref lines))
                ret.Add("notification_sys", lines);
            if (!String.IsNullOrEmpty(userInfo.script_notification_sys))
                if (ParseSettings((string[])Settings.Store.notification_sys, ref lines))
                    ret.Add("notification_sys", lines);
            if (ParseSettings((string[])Settings.Store.notification_usr, ref lines))
                ret.Add("notification_usr", lines);
            if (!String.IsNullOrEmpty(userInfo.script_notification_usr))
                if (ParseSettings((string[])Settings.Store.notification_usr, ref lines))
                    ret.Add("notification_usr", lines);

            if (ParseSettings((string[])Settings.Store.changepwd_sys, ref lines))
                ret.Add("changepwd_sys", lines);
            if (!String.IsNullOrEmpty(userInfo.script_changepwd_sys))
                if (ParseSettings((string[])Settings.Store.changepwd_sys, ref lines))
                    ret.Add("changepwd_sys", lines);
            if (ParseSettings((string[])Settings.Store.changepwd_usr, ref lines))
                ret.Add("changepwd_usr", lines);
            if (!String.IsNullOrEmpty(userInfo.script_changepwd_usr))
                if (ParseSettings((string[])Settings.Store.changepwd_usr, ref lines))
                    ret.Add("changepwd_usr", lines);

            return ret;
        }

        internal bool ParseSettings(string[] setting, ref Dictionary<bool, string> lines)
        {
            lines = new Dictionary<bool, string>();
            foreach (string line in setting)
            {
                if (Regex.IsMatch(line, "(?i)(True|False)\t.*"))
                {
                    string[] split = line.Split('\t');
                    if (split.Length == 2)
                    {
                        lines.Add(Convert.ToBoolean(split[0]), split[1]);
                    }
                }
            }

            return Convert.ToBoolean(lines.Count);
        }

        internal bool ParseSettings(string[] setting, ref List<notify> lines)
        {
            lines = new List<notify>();
            foreach (string line in setting)
            {
                if (Regex.IsMatch(line, "(?i)(True|False)\t.*"))
                {
                    string[] split = line.Split('\t');
                    if (split.Length == 2)
                    {
                        notify notification = new notify();
                        notification.pwd = Convert.ToBoolean(split[0]);
                        notification.logon = false;
                        notification.logoff = false;
                        notification.script = split[1];
                        lines.Add(notification);
                    }
                }
                if (Regex.IsMatch(line, "(?i)(True|False)\t(True|False)\t(True|False)\t.*"))
                {
                    string[] split = line.Split('\t');
                    if (split.Length == 4)
                    {
                        notify notification = new notify();
                        notification.pwd = Convert.ToBoolean(split[0]);
                        notification.logon = Convert.ToBoolean(split[1]);
                        notification.logoff = Convert.ToBoolean(split[2]);
                        notification.script = split[3];
                        lines.Add(notification);
                    }
                }
            }

            return Convert.ToBoolean(lines.Count);
        }

        internal bool Run(int sessionId, string cmd, UserInformation userInfo, bool pwd, bool? sys, string authentication, string authorization, string gateway)
        {
            string expand_cmd = "";
            string expand_cmd_out = "";
            foreach (string c in cmd.Split(' '))
            {
                string ce = c;
                string cp = c;
                if (!Regex.IsMatch(c, @"(?i)%\S+%") && Regex.IsMatch(c, @"(?i)%\S+"))
                {
                    ce = ce.Replace("%u", userInfo.Username);
                    cp = cp.Replace("%u", userInfo.Username);
                    ce = ce.Replace("%o", userInfo.OriginalUsername);
                    cp = cp.Replace("%o", userInfo.OriginalUsername);
                    if (pwd)
                    {
                        ce = ce.Replace("%p", userInfo.Password);
                        cp = cp.Replace("%p", "*****");
                        ce = ce.Replace("%b", userInfo.oldPassword);
                        cp = cp.Replace("%b", "*****");
                    }
                    ce = ce.Replace("%s", userInfo.SID.Value);
                    cp = cp.Replace("%s", userInfo.SID.Value);
                    ce = ce.Replace("%e", userInfo.PasswordEXP.ToString());
                    cp = cp.Replace("%e", userInfo.PasswordEXP.ToString());
                    ce = ce.Replace("%i", userInfo.SessionID.ToString());
                    cp = cp.Replace("%i", userInfo.SessionID.ToString());

                    ce = ce.Replace("%Ae", "Ae:" + authentication);
                    cp = cp.Replace("%Ae", "Ae:" + authentication);
                    ce = ce.Replace("%Ao", "Ao:" + authorization);
                    cp = cp.Replace("%Ao", "Ao:" + authorization);
                    ce = ce.Replace("%Gw", "Gw:" + gateway);
                    cp = cp.Replace("%Gw", "Gw:" + gateway);
                    expand_cmd += ce + " ";
                    expand_cmd_out += cp + " ";
                }
                else
                {
                    expand_cmd += c + " ";
                    expand_cmd_out += cp + " ";
                }
            }
            expand_cmd = expand_cmd.Trim();
            expand_cmd_out = expand_cmd_out.Trim();
            m_logger.InfoFormat("execute {0}", expand_cmd_out);

            if (sys == true)
            {
                return (Abstractions.WindowsApi.pInvokes.CProcess(null, expand_cmd) == 0) ? true : false;
            }
            else if (sys == false)
            {
                return Abstractions.WindowsApi.pInvokes.StartUserProcessInSessionWait(sessionId, expand_cmd);
            }
            else //null while logoff
            {
                return Abstractions.WindowsApi.pInvokes.StartProcessAsUserWait(userInfo.Username, Environment.MachineName, userInfo.Password, expand_cmd);
            }
        }

        internal string GetAuthenticationPluginResults(SessionProperties properties)
        {
            string authe = "";
            try
            {
                PluginActivityInformation pluginInfo = properties.GetTrackedSingle<PluginActivityInformation>();
                foreach (Guid uuid in pluginInfo.GetAuthenticationPlugins())
                {
                    if (pluginInfo.GetAuthenticationResult(uuid).Success)
                        authe += "{" + uuid + "}";
                }
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("GetAuthenticationPluginResults Exception:", ex);
            }

            return authe;
        }

        internal string GetAuthorizationResults(SessionProperties properties)
        {
            string autho = "";
            try
            {
                PluginActivityInformation pluginInfo = properties.GetTrackedSingle<PluginActivityInformation>();
                foreach (Guid uuid in pluginInfo.GetAuthorizationPlugins())
                {
                    if (pluginInfo.GetAuthorizationResult(uuid).Success)
                        autho += "{" + uuid + "}";
                }
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("GetAuthorizationResults Exception:", ex);
            }

            return autho;
        }

        internal string GetGatewayResults(SessionProperties properties)
        {
            string gateway = "";
            try
            {
                PluginActivityInformation pluginInfo = properties.GetTrackedSingle<PluginActivityInformation>();
                foreach (Guid uuid in pluginInfo.GetGatewayPlugins())
                {
                    if (pluginInfo.GetGatewayResult(uuid).Success)
                        gateway += "{" + uuid + "}";
                }
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("GetGatewayResults Exception:", ex);
            }

            return gateway;
        }

        public BooleanResult AuthenticateUser(SessionProperties properties)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            Dictionary<string, List<notify>> settings = GetSettings(userInfo);
            List<notify> authe_sys = new List<notify>();
            try { authe_sys = settings["authe_sys"]; }
            catch { }

            foreach (notify line in authe_sys)
            {
                if (!Run(userInfo.SessionID, line.script, userInfo, line.pwd, true, GetAuthenticationPluginResults(properties), "", ""))
                    return new BooleanResult { Success = false, Message = String.Format("failed to run:{0}", line.script) };
            }

            // return false if no other plugin succeeded
            BooleanResult ret = new BooleanResult() { Success = false, Message = this.Name + " plugin can't authenticate a user on its own" };
            PluginActivityInformation pluginInfo = properties.GetTrackedSingle<PluginActivityInformation>();
            foreach (Guid uuid in pluginInfo.GetAuthenticationPlugins())
            {
                if (pluginInfo.GetAuthenticationResult(uuid).Success)
                {
                    return new BooleanResult() { Success = true };
                }
                else
                {
                    ret.Message = pluginInfo.GetAuthenticationResult(uuid).Message;
                }
            }

            return ret;
        }

        public BooleanResult AuthorizeUser(SessionProperties properties)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            Dictionary<string, List<notify>> settings = GetSettings(userInfo);
            List<notify> autho_sys = new List<notify>();
            try { autho_sys = settings["autho_sys"]; }
            catch { }


            foreach (notify line in autho_sys)
            {
                if (!Run(userInfo.SessionID, line.script, userInfo, line.pwd, true, GetAuthenticationPluginResults(properties), GetAuthorizationResults(properties), ""))
                    return new BooleanResult { Success = false, Message = String.Format("failed to run:{0}", line.script) };
            }

            // return false if no other plugin succeeded
            BooleanResult ret = new BooleanResult() { Success = false, Message = this.Name + " plugin can't authorize a user on its own" };
            PluginActivityInformation pluginInfo = properties.GetTrackedSingle<PluginActivityInformation>();
            foreach (Guid uuid in pluginInfo.GetAuthorizationPlugins())
            {
                if (pluginInfo.GetAuthorizationResult(uuid).Success)
                {
                    return new BooleanResult() { Success = true };
                }
                else
                {
                    ret.Message = pluginInfo.GetAuthorizationResult(uuid).Message;
                }
            }

            return ret;
        }

        public BooleanResult AuthenticatedUserGateway(SessionProperties properties)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            Dictionary<string, List<notify>> settings = GetSettings(userInfo);
            List<notify> gateway_sys = new List<notify>();
            try { gateway_sys = settings["gateway_sys"]; }
            catch { }

            foreach (notify line in gateway_sys)
            {
                if (!Run(userInfo.SessionID, line.script, userInfo, line.pwd, true, GetAuthenticationPluginResults(properties), GetAuthorizationResults(properties), GetGatewayResults(properties)))
                    return new BooleanResult { Success = false, Message = String.Format("failed to run:{0}", line.script) };
            }

            return new BooleanResult() { Success = true };
        }

        public void SessionChange(int SessionId, System.ServiceProcess.SessionChangeReason Reason, SessionProperties properties)
        {
            if (properties == null)
                return;

            if (Reason == System.ServiceProcess.SessionChangeReason.SessionLogoff)
            {
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
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
                }
            }
            if (Reason == System.ServiceProcess.SessionChangeReason.SessionLogon || Reason == System.ServiceProcess.SessionChangeReason.SessionLogoff)
            {
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
                if (userInfo.Description.Contains("pGina created"))
                {
                    Dictionary<string, List<notify>> settings = GetSettings(userInfo);
                    List<notify> notification_sys = new List<notify>();
                    try { notification_sys = settings["notification_sys"]; }
                    catch { }
                    List<notify> notification_usr = new List<notify>();
                    try { notification_usr = settings["notification_usr"]; }
                    catch { }

                    string authe = GetAuthenticationPluginResults(properties);
                    string autho = GetAuthorizationResults(properties);
                    string gateway = GetGatewayResults(properties);

                    foreach (notify line in notification_sys)
                    {
                        if (Reason == System.ServiceProcess.SessionChangeReason.SessionLogon && line.logon)
                        {
                            if (!Run(userInfo.SessionID, line.script, userInfo, line.pwd, true, authe, autho, gateway))
                                m_logger.InfoFormat("failed to run:{0}", line.script);
                        }
                    }

                    foreach (notify line in notification_usr)
                    {
                        if (Reason == System.ServiceProcess.SessionChangeReason.SessionLogon && line.logon)
                        {
                            if (!Run(userInfo.SessionID, line.script, userInfo, line.pwd, false, authe, autho, gateway))
                                m_logger.InfoFormat("failed to run:{0}", line.script);
                        }
                    }

                    if (Reason == System.ServiceProcess.SessionChangeReason.SessionLogoff)
                    {
                        Thread rem_smb = new Thread(() => cleanup(userInfo, SessionId, properties, notification_sys, notification_usr, Reason, authe, autho, gateway));
                        rem_smb.Start();
                    }
                }
            }
        }

        public BooleanResult ChangePassword(SessionProperties properties, ChangePasswordPluginActivityInfo pluginInfo)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            Dictionary<string, List<notify>> settings = GetSettings(userInfo);
            List<notify> changepwd_sys = new List<notify>();
            try { changepwd_sys = settings["changepwd_sys"]; }
            catch { }
            List<notify> changepwd_usr = new List<notify>();
            try { changepwd_usr = settings["changepwd_usr"]; }
            catch { }

            foreach (notify line in changepwd_sys)
            {
                if (!Run(userInfo.SessionID, line.script, userInfo, line.pwd, true, GetAuthenticationPluginResults(properties), GetAuthorizationResults(properties), GetGatewayResults(properties)))
                    return new BooleanResult { Success = false, Message = String.Format("failed to run:{0}", line.script) };
            }
            foreach (notify line in changepwd_usr)
            {
                if (!Run(userInfo.SessionID, line.script, userInfo, line.pwd, false, GetAuthenticationPluginResults(properties), GetAuthorizationResults(properties), GetGatewayResults(properties)))
                    return new BooleanResult { Success = false, Message = String.Format("failed to run:{0}", line.script) };
            }

            // the change password plugin chain will end as soon as one plugin failed
            // no special treatment needed
            return new BooleanResult { Success = true };
        }

        private void cleanup(UserInformation userInfo, int sessionID, SessionProperties properties, List<notify> notification_sys, List<notify> notification_usr, System.ServiceProcess.SessionChangeReason Reason, string authentication, string authorization, string gateway)
        {
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

                foreach (notify line in notification_sys)
                {
                    if (line.logoff)
                    {
                        if (!Run(userInfo.SessionID, line.script, userInfo, line.pwd, true, authentication, authorization, gateway))
                            m_logger.InfoFormat("failed to run:{0}", line.script);
                    }
                }

                foreach (notify line in notification_usr)
                {
                    if (line.logoff)
                    {
                        if (!Run(userInfo.SessionID, line.script, userInfo, line.pwd, null, authentication, authorization, gateway))
                            m_logger.InfoFormat("failed to run:{0}", line.script);
                    }
                }

                m_logger.InfoFormat("{0} scripting done", userInfo.Username);
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
