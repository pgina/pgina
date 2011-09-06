using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Settings;

namespace pGina.Plugin.SingleUser
{
    public class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);

        static Settings()
        {
            m_settings.SetDefault("Username", "Username");
            m_settings.SetDefault("Domain", Environment.MachineName);
            m_settings.SetDefault("Password", "Password");
            m_settings.SetDefault("RequirePlugins", false);
            m_settings.SetDefault("RequiredPluginList", new string[] { });
        }

        public static dynamic Store
        {
            get { return m_settings; }
        }
    }
}
