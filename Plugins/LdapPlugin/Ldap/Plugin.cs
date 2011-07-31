using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using log4net;

using pGina.Shared;
using pGina.Shared.Interfaces;
using pGina.Shared.Settings;

namespace pGina.Plugin.Ldap
{
    public class LdapPlugin : IPluginAuthentication, IPluginConfiguration
    {
        public static readonly Guid LdapUuid = new Guid("{0F52390B-C781-43AE-BD62-553C77FA4CF7}");

        private dynamic m_settings = null;
        private ILog m_logger = LogManager.GetLogger("LdapPlugin");
        
        public LdapPlugin()
        {
            InitSettings();

            using(Process me = Process.GetCurrentProcess())
            {
                m_logger.DebugFormat("LDAP Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }

        private void InitSettings()
        {
            m_settings = new DynamicSettings(LdapUuid);

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
            m_settings.SetDefault("SearchPW", "");
            m_settings.SetDefault("DoGroupAuthorization", false);
            m_settings.SetDefault("LdapLoginGroups", new string[] { });
            m_settings.SetDefault("LdapAdminGroup", "wheel");
        }

        public string Name
        {
            get { return "LDAP Authentication"; }
        }

        public string Description
        {
            get { return "A plugin that authenticates logins via an LDAP server."; }
        }

        public string Version
        {
            get { return "1.0.0"; }
        }

        public Guid Uuid
        {
            get { return LdapUuid; }
        }
        
        public BooleanResult AuthenticateUser(Shared.Types.SessionProperties properties)
        {
            try
            {
                m_logger.DebugFormat("AuthenticateUser({0})", properties.Id.ToString());

                Shared.Types.UserInformation userInfo = properties.GetTrackedSingle<Shared.Types.UserInformation>();

                m_logger.DebugFormat("Received username: {0}", userInfo.Username);

                // Place credentials into a NetworkCredentials object
                NetworkCredential creds = new NetworkCredential(userInfo.Username, userInfo.Password);

                // Authenticate the login
                m_logger.DebugFormat("Attempting authentication for {0}", creds.UserName);
                LdapAuthenticator authenticator = new LdapAuthenticator(creds);
                return authenticator.Authenticate();
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("AuthenticateUser exception: {0}", e);
                throw;  // Allow pGina service to catch and handle exception
            }
        }        

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }
    }
}
