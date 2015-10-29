using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Settings;

namespace pGina.Plugin.HttpAuth
{
    class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);

        static Settings()
        {
            m_settings.SetDefault("Loginserver", @"https://server.my.domain.com/%u");
        }

        public static dynamic Store
        {
            get { return m_settings; }
        }
    }
}
