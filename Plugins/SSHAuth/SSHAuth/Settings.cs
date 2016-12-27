using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Settings;

namespace pGina.Plugin.SSHAuth
{
    public class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);

        static Settings()
        {
            m_settings.SetDefault("Host", "localhost");
            m_settings.SetDefault("Port", "22");
        }

        public static dynamic Store
        {
            get { return m_settings; }
        }
    }
}
