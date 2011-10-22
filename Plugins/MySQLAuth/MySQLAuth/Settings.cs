using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Plugin.MySQLAuth
{
    class Settings
    {
        private static dynamic m_settings = new pGina.Shared.Settings.pGinaDynamicSettings(PluginImpl.PluginUuid);
        public static dynamic Store
        {
            get { return m_settings; }
        }

        static Settings()
        {
            m_settings.SetDefault("Host", "localhost");
            m_settings.SetDefault("Port", 3306);
            m_settings.SetDefault("UseSsl", false);
            m_settings.SetDefault("User", "mysql_admin_user");
            m_settings.SetDefaultEncryptedSetting("Password", "secret");
            m_settings.SetDefault("Database", "account_db");
            m_settings.SetDefault("Table", "users");
        }
    }
}
