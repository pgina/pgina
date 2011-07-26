using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using log4net;

using pGina.Shared;
using pGina.Shared.Interfaces;
using pGina.Shared.AuthenticationUI;

namespace pGina.Plugin.Ldap
{
    public class LdapPlugin : IPluginAuthentication, IPluginAuthenticationUI
    {
        private ILog m_logger = LogManager.GetLogger("LdapPlugin");
        internal static LdapPluginSettings Settings = null;

        private Guid m_descriptionGuid = new Guid("{5FF90A12-8C06-4E21-8C99-3B01E893F430}");        

        public LdapPlugin()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                m_logger.DebugFormat("LDAP Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
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
            get { return new Guid("{9C758C53-BDF2-446A-A927-B359B697CDA5}"); }
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

                // (Re)load settings
                Settings = LdapPluginSettings.Load();

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
    }
}
