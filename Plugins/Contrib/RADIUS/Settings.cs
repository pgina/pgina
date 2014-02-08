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
            m_settings.SetDefault("EnableAuth", true);
            m_settings.SetDefault("EnableAcct", false);
            
            m_settings.SetDefault("Server", "");
            m_settings.SetDefault("AuthPort", 1812); //Authentication port
            m_settings.SetDefault("AcctPort", 1813); //Authorization port
            m_settings.SetDefaultEncryptedSetting("SharedSecret", "");
            m_settings.SetDefault("Timeout", 2500); //in ms
            m_settings.SetDefault("Retry", 3);

            m_settings.SetDefault("SendNASIPAddress", true);
            m_settings.SetDefault("SendNASIdentifier", true);
            m_settings.SetDefault("NASIdentifier", "%computername");
            m_settings.SetDefault("SendCalledStationID", false);
            m_settings.SetDefault("CalledStationID", "%macaddr");

            m_settings.SetDefault("AcctingForAllUsers", false);
            m_settings.SetDefault("SendInterimUpdates", false);
            m_settings.SetDefault("ForceInterimUpdates", false);
            m_settings.SetDefault("InterimUpdateTime", 900); //900sec = 15 min

            m_settings.SetDefault("AllowSessionTimeout", false);
            m_settings.SetDefault("WisprSessionTerminate", false);
            

            m_settings.SetDefault("UseModifiedName", false);
            m_settings.SetDefault("IPSuggestion", "");
            
        }
    }
}
