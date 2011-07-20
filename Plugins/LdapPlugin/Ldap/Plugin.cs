using System;
using System.Collections.Generic;
using System.Diagnostics;

using log4net;

using pGina.Interfaces;

namespace pGina.Plugin.Ldap
{
    public class LdapPlugin : IPluginAuthentication, IPluginAuthenticationUI
    {
        private ILog m_logger = LogManager.GetLogger("LdapPlugin");
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
            get { return "LDAP Authentication Plugin"; }
        }

        public string Description
        {
            get { return "A plugin that authenticates logins via an LDAP server."; }
        }

        public Guid Uuid
        {
            get { return new Guid("{9C758C53-BDF2-446A-A927-B359B697CDA5}"); }
        }
        
        public AuthenticationResult AuthenticateUser(Interfaces.AuthenticationUI.Element[] values, Guid trackingToken)
        {
            // TODO
            return null; 
        }

        public void SetupUI(List<Interfaces.AuthenticationUI.Element> elements)
        {
            throw new NotImplementedException();
        }
    }
}
