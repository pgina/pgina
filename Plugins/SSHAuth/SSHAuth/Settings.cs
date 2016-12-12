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
            m_settings.SetDefault("Foo", "Bar");
            m_settings.SetDefault("DoSomething", true);
            m_settings.SetDefault("ListOfStuff", new string[] { "a", "b", "c" });
            m_settings.SetDefault("Size", 1);
        }

        public static dynamic Store
        {
            get { return m_settings; }
        }
    }
}
