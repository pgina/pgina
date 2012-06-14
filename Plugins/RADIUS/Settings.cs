using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Settings;

namespace pGina.Plugin.RADIUS
{
    class Settings
    {

        public static dynamic Store
        {
            get { return m_settings; }
        }
        private static dynamic m_settings;

        static Settings()
        {
            m_settings = new pGinaDynamicSettings(RADIUSPlugin.SimpleUuid);

            // Set default values for settings (if not already set)
            m_settings.SetDefault("Server", "");
            //m_settings.SetDefault("UseSsl", true);
            //m_settings.SetDefault("Protocol", "");
            m_settings.SetDefault("Port", "1812");
            m_settings.SetDefaultEncryptedSetting("SharedSecret", "");
            m_settings.SetDefault("Timeout", 3000); //in ms
        }
    }
}
