using System;
using System.DirectoryServices.AccountManagement;
using System.Diagnostics;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using log4net;

namespace pGina.Plugin.HttpAuth
{
    public class PluginImpl : IPluginConfiguration, IPluginAuthentication, IPluginAuthenticationGateway, IPluginChangePassword
    {
        private static ILog m_logger = LogManager.GetLogger("HttpAuth");

        #region Init-plugin
        public static Guid PluginUuid
        {
            get { return new Guid("{C4FF794F-5843-4BEF-90BA-B5E4E488C662}"); }
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
            get { return "HttpAuth"; }
        }

        public string Description
        {
            get { return "Uses http(s) request to obtain user info"; }
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
            Configuration dialog = new Configuration();
            dialog.ShowDialog();
        }

        public BooleanResult AuthenticateUser(SessionProperties properties)
        {
            // this method shall say if our credentials are valid
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            return HttpAccessor.getResponse(userInfo.Username, userInfo.Password);
        }

        public BooleanResult AuthenticatedUserGateway(SessionProperties properties)
        {
            // this method shall perform some other tasks ...

            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            UInfo uinfo = HttpAccessor.getUserInfo(userInfo.Username);
            if (uinfo != null)
            {
                m_logger.DebugFormat("AuthenticatedUserGateway: Uinfo: {0}", uinfo.ToString());
                foreach (string group in uinfo.groups)
                {
                    userInfo.AddGroup(new GroupInformation() { Name = group });
                }
                properties.AddTrackedSingle<UserInformation>(userInfo);

                // and what else ??? :)
                
            }

            return new BooleanResult() { Success = true };
        }

        public BooleanResult ChangePassword(SessionProperties properties, ChangePasswordPluginActivityInfo pluginInfo)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            m_logger.DebugFormat("ChangePassword(): {0}", userInfo.ToString());

            // Verify the old password
            if (Abstractions.WindowsApi.pInvokes.ValidateCredentials(userInfo.Username, userInfo.oldPassword))
            {
                m_logger.DebugFormat("Authenticated via old password: {0}", userInfo.Username);
            }
            else
            {
                return new BooleanResult { Success = false, Message = "Current password or username is not valid." };
            }

            return HttpAccessor.getPwChangeResponse(userInfo.Username, userInfo.Password, userInfo.oldPassword);
        }
    }
}
