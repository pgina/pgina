using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pGina.Shared.Settings;

namespace pGina.Plugin.scripting
{
    public class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);

        static Settings()
        {
            // Authentication step settings:
            m_settings.SetDefault("authe_sys", new string[] { });

            // Authorization step settings
            m_settings.SetDefault("autho_sys", new string[] { });

            // Gateway step settings
            m_settings.SetDefault("gateway_sys", new string[] { });

            // Notification
            m_settings.SetDefault("notification_sys", new string[] { });
            m_settings.SetDefault("notification_usr", new string[] { });

            // change password
            m_settings.SetDefault("changepwd_sys", new string[] { });
            m_settings.SetDefault("changepwd_usr", new string[] { });
        }

        public static dynamic Store
        {
            get { return m_settings; }
        }
    }
}
