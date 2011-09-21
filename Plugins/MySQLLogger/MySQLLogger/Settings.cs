using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Settings;

namespace pGina.Plugin.MySqlLogger
{
    

    class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);
        public static dynamic Store
        {
            get { return m_settings; }
        }

        static Settings()
        {
            // Set defaults
            m_settings.SetDefault("Host", "localhost");
            m_settings.SetDefault("Port", 3306);
            m_settings.SetDefault("User", "pGina");
            m_settings.SetDefaultEncryptedSetting("Password", "secret", null);
            m_settings.SetDefault("Database", "pGinaDB");

            m_settings.SetDefault("EvtLogon", true);
            m_settings.SetDefault("EvtLogoff", true);
            m_settings.SetDefault("EvtLock", false);
            m_settings.SetDefault("EvtUnlock", false);
            m_settings.SetDefault("EvtConsoleConnect",false);
            m_settings.SetDefault("EvtConsoleDisconnect", false);
            m_settings.SetDefault("EvtRemoteControl", false);
            m_settings.SetDefault("EvtRemoteConnect", false);
            m_settings.SetDefault("EvtRemoteDisconnect", false);
        }

    }
}
