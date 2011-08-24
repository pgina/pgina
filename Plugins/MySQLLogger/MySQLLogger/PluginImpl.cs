using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Interfaces;
using pGina.Shared.Settings;
using log4net;

namespace pGina.Plugin.MySqlLogger
{
    public class PluginImpl : IPluginSystemSessionHelper, IPluginConfiguration
    {
        public static readonly Guid PluginUuid = new Guid("B68CF064-9299-4765-AC08-ACB49F93F892");
        private ILog m_logger = LogManager.GetLogger("MySqlLoggerPlugin");

        public static readonly string TABLE_NAME = "pGinaLog";

        private static dynamic m_settings = null;
        internal static dynamic Settings
        {
            get { return m_settings; }
        }

        static PluginImpl()
        {
            m_settings = new DynamicSettings(PluginUuid);
            // Set defaults
            m_settings.SetDefault("Host", "localhost");
            m_settings.SetDefault("Port", 3306);
            m_settings.SetDefault("User", "pGina");
            m_settings.SetDefault("Password", "secret");
            m_settings.SetDefault("Database", "pGinaDB");
        }

        public void SessionEnding(Shared.Types.SessionProperties properties)
        {
            m_logger.Debug("SessionEnding()");
        }

        public void SessionStarted(Shared.Types.SessionProperties properties)
        {
            m_logger.Debug("SessionStarted()");

            Shared.Types.UserInformation userInfo = properties.GetTrackedSingle<Shared.Types.UserInformation>();
            try
            {
                using (DbLogger db = DbLogger.Connect())
                {
                    db.Log(String.Format("Login: user={0}", userInfo.Username));
                }
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("{0}: {1}", e.GetType().ToString(), e.Message);
            }
        }

        public string Description
        {
            get { return "Logs various events to a MySQL database."; }
        }

        public string Name
        {
            get { return "MySQL Logger"; }
        }

        public Guid Uuid
        {
            get { return PluginUuid; }
        }

        public string Version
        {
            get { return "1.0.0"; }
        }

        public void Configure()
        {
            Configuration dlg = new Configuration();
            dlg.ShowDialog();
        }
    }
}
