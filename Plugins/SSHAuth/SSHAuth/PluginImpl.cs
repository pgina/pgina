using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using log4net;
using pGina.Shared.Interfaces;
using pGina.Shared.Types;

namespace pGina.Plugin.SSHAuth
{
    public class PluginImpl : IPluginAuthentication, IPluginAuthorization, IPluginAuthenticationGateway ,IPluginChangePassword, IPluginConfiguration
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
            Configuration myDialog = new Configuration();
            myDialog.ShowDialog();
        }

        public BooleanResult AuthenticateUser(SessionProperties properties)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            if (userInfo.Username.Contains("hello") && userInfo.Password.Contains("pGina"))
            {
                // Successful authentication
                return new BooleanResult() { Success = true };
            }
            // Authentication failure
            return new BooleanResult() { Success = false, Message = "Incorrect username or password." };
        }

        public BooleanResult AuthorizeUser(SessionProperties properties)
        {
            return new BooleanResult() { Success = false, Message = "AuthorizeUser test fail" };
        }

        public BooleanResult AuthenticatedUserGateway(SessionProperties properties)
        {
            return new BooleanResult() { Success = false, Message = "AuthenticatedUserGateway test fail" };
        }

        public BooleanResult ChangePassword(SessionProperties properties, ChangePasswordPluginActivityInfo pluginInfo)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            if (userInfo.HasSID)
            {
                return new BooleanResult()
                {
                    Success = false,
                    Message = String.Format("This is an error message")
                };
            }

            return new BooleanResult() { Success = true };
        }
    }
}
