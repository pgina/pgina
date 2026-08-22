using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenCredential.Shared.Settings;

namespace OpenCredential.Plugin.Email
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
            m_settings = new OpenCredentialDynamicSettings(EmailAuthPlugin.SimpleUuid);

            // Set default values for settings (if not already set)
            m_settings.SetDefault("Server", "");
            m_settings.SetDefault("UseSsl", true);
            m_settings.SetDefault("Protocol", "");
            m_settings.SetDefault("Port", "");
            m_settings.SetDefault("AppendDomain", false);
            m_settings.SetDefault("Domain", "");
            m_settings.SetDefault("NetworkTimeout", 10000);  // timeout in ms
        }
    }
}
