/*
    Written by Florian Rohmer (2013)
     
    Distribued under the pGina license.
    All rights reserved.
 
	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors 
		  may be used to endorse or promote products derived from this software without 
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.DirectoryServices.Protocols;

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using pGina.Shared.Settings;

namespace pGina.Plugin.SingleUserSwitcher
{
    public class PluginImpl : IPluginConfiguration, IPluginAuthenticationGateway
    {

        private ILog m_logger = LogManager.GetLogger("SingleUserSwitcher_Plugin");
        public static Guid PluginUuid = new Guid("{7231F5C3-EA88-4790-8733-65A904CBD967}");        
        
        public PluginImpl()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                Settings.Init();
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }        

        public string Name
        {
            get { return "Single User Switcher"; }
        }

        public string Description
        {
            get { return "Evolution of Single User Login. Can switch between different sessions depending on ldap group."; }
        }

        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public Guid Uuid
        {
            get { return PluginUuid; }
        }
                
        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }

        private bool DidPluginAuth(string uuid, SessionProperties properties)
        {
            try
            {
                Guid pluginUuid = new Guid(uuid);
                PluginActivityInformation pluginInfo = properties.GetTrackedSingle<PluginActivityInformation>();
                return pluginInfo.GetAuthenticationResult(pluginUuid).Success;
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Unable to validate that {0} authenticated user: {1}", uuid, e);
                return false;
            }
        }

        public BooleanResult AuthenticatedUserGateway(SessionProperties properties)
        {
            m_logger.DebugFormat("AuthenticatedUserGateway[{0}]", properties.GetTrackedSingle<UserInformation>().Username);

            string defaultUsername = Settings.Store.Username;
            string domain = Settings.Store.Domain;
            string defaultPassword = Settings.Store.GetEncryptedSetting("Password", null);
            bool requirePlugins = Settings.Store.RequirePlugins;
            bool requireAllPlugins = Settings.Store.RequireAllPlugins;
            string[] pluginList = Settings.Store.RequiredPluginList;

            // Do we have to check for a specific plugin(s)?
            if (requirePlugins && pluginList.Length > 0)
            {
                //Requires all plugins
                if (requireAllPlugins)
                {
                    foreach (string pluginUuid in pluginList)
                    {
                        m_logger.DebugFormat("Checking whether {0} authenticated the user", pluginUuid);
                        if (!DidPluginAuth(pluginUuid, properties))
                            return new BooleanResult() { Success = true };  // Silent bypass
                    }
                }

                //Requires any plugin
                else
                {
                    bool matchFound = false;
                    foreach (string pluginUuid in pluginList)
                    {
                        matchFound = DidPluginAuth(pluginUuid, properties);
                        if (matchFound)
                            break;
                    }

                    if (!matchFound)
                        return new BooleanResult() { Success = true }; //Silent bypass
                }
            }



            /* LOOKING FOR ALTERNATIVE SESSION TO OPEN */

            // the session to open if not the default one
            Session sessionToKeep = null;
            // shows if we found a session to open
            bool aSessionIsFound = false;

            List<Session> sessList = Session.GetSessions();
            if (sessList.Count() != 0) // we have to check user's membership
            {
                BooleanResult connectionBoolResult = NewLdapConnection(properties);
                if (!connectionBoolResult.Success) return connectionBoolResult;

                LdapServer serv = properties.GetTrackedSingle<LdapServer>();

                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
                string user = userInfo.Username;

                foreach (Session sess in sessList)
                {
                    if (!aSessionIsFound) 
                    {
                        bool inGroup = serv.MemberOfGroup(user, sess.ldapgroup);
                        if (inGroup)
                        {
                            sessionToKeep = sess; // we keep the first occurence of matching ldap group
                            aSessionIsFound = true;
                            m_logger.DebugFormat("{0} is a member of {1} so we will eventually open the session {2}", user, sess.ldapgroup, sess.username);
                        }
                    }
                }

                // closing the connection
                if (serv != null) serv.Close();
            }

            /* CREDENTIALS SUBSTITUTION */
            if (aSessionIsFound) // we open alternative session
            {
                // Substitute with alternative session
                m_logger.DebugFormat("Re-writing user information to login with {0}\\{1}", domain, sessionToKeep.username);
                properties.GetTrackedSingle<UserInformation>().Username = sessionToKeep.username;
                properties.GetTrackedSingle<UserInformation>().Domain = domain;
                properties.GetTrackedSingle<UserInformation>().Password = sessionToKeep.password;
                return new BooleanResult() { Success = true };
            }
            else // we open default session
            {
                // Substitute with DEFAULT SESSION
                m_logger.DebugFormat("Authenticated user is not a member of any group given in the plugin config, so we will open the default session.");
                m_logger.DebugFormat("Re-writing user information to login with {0}\\{1}", domain, defaultUsername);
                properties.GetTrackedSingle<UserInformation>().Username = defaultUsername;
                properties.GetTrackedSingle<UserInformation>().Domain = domain;
                properties.GetTrackedSingle<UserInformation>().Password = defaultPassword;
                return new BooleanResult() { Success = true };
            }

        }

        /// <summary>
        /// binds to LDAP
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        private BooleanResult NewLdapConnection(SessionProperties properties)
        {
            try
            {
                LdapServer serv = new LdapServer();
                properties.AddTrackedSingle<LdapServer>(serv);
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Failed to create LdapServer: {0}", e);
                properties.AddTrackedSingle<LdapServer>(null);
            }


            // Get the LdapServer object from the session properties 
            LdapServer server = properties.GetTrackedSingle<LdapServer>();
            if (server == null)
                return new BooleanResult() { Success = false, Message = "Internal error: LdapServer object not available" };

            try
            {
                m_logger.DebugFormat("AuthenticateUser({0})", properties.Id.ToString());
                Shared.Types.UserInformation userInfo = properties.GetTrackedSingle<Shared.Types.UserInformation>();
                m_logger.DebugFormat("Received username: {0}", userInfo.Username);

                // Authenticate the login
                m_logger.DebugFormat("Attempting authentication for {0}", userInfo.Username);
                return server.Authenticate(userInfo.Username, userInfo.Password);
            }
            catch (Exception e)
            {
                if (e is LdapException)
                {
                    LdapException ldapEx = (e as LdapException);

                    if (ldapEx.ErrorCode == 81)
                    {
                        // Server can't be contacted, set server object to null
                        m_logger.ErrorFormat("Server unavailable: {0}, {1}", ldapEx.ServerErrorMessage, e.Message);
                        server.Close();
                        properties.AddTrackedSingle<LdapServer>(null);
                        return new BooleanResult { Success = false, Message = "Failed to contact LDAP server." };
                    }
                }

                // This is an unexpected error, so set LdapServer object to null, because
                // subsequent stages shouldn't use it, and this indicates to later stages
                // that this stage failed unexpectedly.
                server.Close();
                properties.AddTrackedSingle<LdapServer>(null);
                m_logger.ErrorFormat("Exception in LDAP authentication: {0}", e);
                throw;  // Allow pGina service to catch and handle exception
            }
        }

        public void Starting() { }
        public void Stopping() { }
    }
}
