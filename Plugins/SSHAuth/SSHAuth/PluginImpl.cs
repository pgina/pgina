using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

using log4net;
using pGina.Shared.Interfaces;
using pGina.Shared.Types;

namespace pGina.Plugin.SSHAuth
{
    public class PluginImpl : IPluginAuthentication, IPluginConfiguration
    {
        private ILog m_logger = LogManager.GetLogger("SSHAuth");

        #region Init-plugin
        public static Guid PluginUuid
        {
            get { return new Guid("{CC35057C-ACA8-499C-B127-AE4CF978F238}"); }
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
            get { return "SSHAuth"; }
        }

        public string Description
        {
            get { return "SSHAuth plugin"; }
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

        public void Configure()
        {
            pGina.Plugin.SSHAuth.Configuration myDialog = new pGina.Plugin.SSHAuth.Configuration();
            myDialog.ShowDialog();
        }

        [DllImport("SSHAuthNative.dll", EntryPoint = "ssh_connect_and_pw_auth", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int ssh_connect_and_pw_authx86(string host, string port, string user, string password, StringBuilder errmsg, int errlen);

        [DllImport("SSHAuthNativex64.dll", EntryPoint = "ssh_connect_and_pw_auth", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int ssh_connect_and_pw_authx64(string host, string port, string user, string password, StringBuilder errmsg, int errlen);

        public static int ssh_connect_and_pw_auth(string host, string port, string user, string password, StringBuilder errmsg, int errlen)
        {
            return IntPtr.Size == 8 /* 64bit */ ? ssh_connect_and_pw_authx64(host, port, user, password, errmsg, errlen) : ssh_connect_and_pw_authx86(host, port, user, password, errmsg, errlen);
        }

        public BooleanResult AuthenticateUser(SessionProperties properties)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            string sshHost = Settings.Store.Host;
            string sshPort = Settings.Store.Port;
            StringBuilder errsb = new StringBuilder(1024);  // For return error message from native auth function
            int rc;

            rc = ssh_connect_and_pw_auth(sshHost, sshPort, userInfo.Username, userInfo.Password, errsb, errsb.Capacity);
            if (rc == 0)
            {
                // Successful authentication
                return new BooleanResult() { Success = true };
            }
            // Authentication failure
            return new BooleanResult() { Success = false, Message = errsb.ToString() };
        }
    }
}
