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
            m_settings.SetDefault("AuthPort", 1812); //Authentication port
            m_settings.SetDefault("AcctPort", 1813); //Authorization port
            m_settings.SetDefaultEncryptedSetting("SharedSecret", "");
            m_settings.SetDefault("Timeout", 2500); //in ms
            m_settings.SetDefault("Retry", 3);
            m_settings.SetDefault("MachineIdentifier", (int)MachineIdentifier.IP_Address);
            m_settings.SetDefault("UseModifiedName", false);
            m_settings.SetDefault("IPSuggestion", "");
            
        }
    }
}
