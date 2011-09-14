using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Settings;

namespace pGina.Plugin.Ldap
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
            m_settings = new pGinaDynamicSettings(LdapPlugin.LdapUuid);

            // Set default values for settings (if not already set)
            m_settings.SetDefault("LdapHost", new string[] { "ldap.example.com" });
            m_settings.SetDefault("LdapPort", 389);
            m_settings.SetDefault("LdapTimeout", 10);
            m_settings.SetDefault("UseSsl", false);
            m_settings.SetDefault("RequireCert", false);
            m_settings.SetDefault("ServerCertFile", "");
            m_settings.SetDefault("DoSearch", false);
            m_settings.SetDefault("SearchContexts", new string[] { });
            m_settings.SetDefault("SearchFilter", "");
            m_settings.SetDefault("DnPattern", "uid=%u,dc=example,dc=com");
            m_settings.SetDefault("SearchDN", "");
            m_settings.SetDefaultEncryptedSetting("SearchPW", "secret", null);
            m_settings.SetDefault("DoGroupAuthorization", false);
            m_settings.SetDefault("LdapLoginGroups", new string[] { });
            m_settings.SetDefault("LdapAdminGroup", "wheel");
        }
    }
}
