using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using log4net;

using pGina.Shared;
using pGina.Shared.Interfaces;
using pGina.Shared.AuthenticationUI;
using pGina.Shared.Settings;

namespace pGina.Plugin.Ldap
{
    public class LdapPlugin : IPluginAuthentication, IPluginAuthenticationUI, IPluginConfiguration
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

        public Guid Uuid
        {
            get { return LdapUuid; }
        }
        
        public BooleanResult AuthenticateUser(Element[] values, Guid trackingToken)
        {
            try
            {
                m_logger.DebugFormat("AuthenticateUser(..., {0})", trackingToken.ToString());

                // Dump UI elements, for debugging
                foreach (Element e in values)
                {
                    m_logger.DebugFormat("Element[{0}]: {1} => {2}", e.UUid, e.Type, e.Name);
                }

                // Get the username text field
                EditTextElement usernameField = (EditTextElement)
                    values.First(v => v.UUid == Constants.UsernameElementUuid);
                // Get the password field
                PasswordTextElement passwordField = (PasswordTextElement)
                    values.First(v => v.UUid == Constants.PasswordElementUuid);

                m_logger.DebugFormat("Received username: {0}", usernameField.Text);

                // Place credentials into a NetworkCredentials object
                NetworkCredential creds = new NetworkCredential(usernameField.Text, passwordField.Text);

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

        public void SetupUI(List<Element> elements)
        {
            try
            {
                // Must have the username|password element            
                if (elements.Where(element => element.UUid == EditTextElement.UsernameElement.UUid).Count() == 0)
                {
                    m_logger.DebugFormat("SetupUI: Adding username element");
                    elements.Add(EditTextElement.UsernameElement);
                }

                if (elements.Where(element => element.UUid == PasswordTextElement.PasswordElement.UUid).Count() == 0)
                {
                    m_logger.DebugFormat("SetupUI: Adding password element");
                    elements.Add(PasswordTextElement.PasswordElement);
                }
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("SetupUI exception: {0}", e);
            }
        }

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }
    }
}
