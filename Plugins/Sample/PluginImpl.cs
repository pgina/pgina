using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using pGina.Shared.Settings;

namespace pGina.Plugin.Sample
{
    public class SimplePlugin : IPluginConfiguration, IPluginAuthentication
    {
        private ILog m_logger = LogManager.GetLogger("SimplePlugin");
        private Guid m_descriptionGuid = new Guid("{22B06063-DEF1-4CE2-96DC-C6FDB409FEFD}");
        public static Guid SimpleUuid = new Guid("{16FC47C0-F17B-4D99-A820-EDBF0B0C764A}");
        private string m_defaultDescription = "A demonstration plugin that allows all usernames that begin with the letter \"p\" to succeed.";
        private dynamic m_settings = null;

        public SimplePlugin()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                m_settings = new DynamicSettings(SimpleUuid);
                m_settings.SetDefault("ShowDescription", true);
                m_settings.SetDefault("Description", m_defaultDescription);
                
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }        

        public string Name
        {
            get { return "Simple Demonstration"; }
        }

        public string Description
        {
            get { return m_settings.Description; }
        }

        public string Version
        {
            get { return "1.0.0"; }
        }

        public Guid Uuid
        {
            get { return SimpleUuid; }
        }
        
        BooleanResult IPluginAuthentication.AuthenticateUser(SessionProperties properties)
        {
            try
            {
                m_logger.DebugFormat("AuthenticateUser({0})", properties.Id.ToString());

                // Get user info
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

                m_logger.DebugFormat("Found username: {0}", userInfo.Username);
                if (userInfo.Username.StartsWith("p"))
                {
                    m_logger.InfoFormat("Authenticated user: {0}", userInfo.Username);
                    return new BooleanResult() { Success = true };
                }

                m_logger.ErrorFormat("Failed to authenticate user: {0}", userInfo.Username);
                return new BooleanResult() { Success = false };
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("AuthenticateUser exception: {0}", e);
                throw;  // Allow pGina service to catch and handle exception
            }
        }

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }
    }
}
