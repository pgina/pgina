/*
	Copyright (c) 2013, pGina Team
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
using System.Diagnostics;
using System.Net;
using System.DirectoryServices.Protocols;

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;

namespace pGina.Plugin.Ldap
{
    public class LdapPlugin : IStatefulPlugin, IPluginAuthentication, IPluginAuthorization, IPluginAuthenticationGateway, IPluginConfiguration, IPluginChangePassword
    {
        public static readonly Guid LdapUuid = new Guid("{0F52390B-C781-43AE-BD62-553C77FA4CF7}");
        private ILog m_logger = LogManager.GetLogger("LdapPlugin");
        
        public LdapPlugin()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                m_logger.DebugFormat("LDAP Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }

        public string Name
        {
            get { return "LDAP"; }
        }

        public string Description
        {
            get { return "Uses a LDAP server as a data source for authentication and/or group authorization."; }
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
            get { return LdapUuid; }
        }
        
        public BooleanResult AuthenticateUser(Shared.Types.SessionProperties properties)
        {
            // Get the LdapServer object from the session properties (created in BeginChain)
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

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }

        public void Starting() { }
        public void Stopping() { }

        public void BeginChain(SessionProperties props)
        {
            m_logger.Debug("BeginChain");
            try
            {
                LdapServer serv = new LdapServer();
                props.AddTrackedSingle<LdapServer>(serv);
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Failed to create LdapServer: {0}", e);
                props.AddTrackedSingle<LdapServer>(null);
            }
        }

        public void EndChain(SessionProperties props)
        {
            m_logger.Debug("EndChain");
            LdapServer serv = props.GetTrackedSingle<LdapServer>();
            if (serv != null) serv.Close();
        }

        public BooleanResult AuthorizeUser(SessionProperties properties)
        {
            m_logger.Debug("LDAP Plugin Authorization");

            bool requireAuth = Settings.Store.AuthzRequireAuth;

            // Get the authz rules from registry
            List<GroupAuthzRule> rules = GroupRuleLoader.GetAuthzRules();
            if (rules.Count == 0)
            {
                throw new Exception("No authorizaition rules found.");
            }
            
            // Get the LDAP server object
            LdapServer serv = properties.GetTrackedSingle<LdapServer>();

            // If LDAP server object is not found, then something went wrong in authentication.
            // We allow or deny based on setting
            if (serv == null)
            {
                m_logger.ErrorFormat("AuthorizeUser: Internal error, LdapServer object not available.");

                // LdapServer is not available, allow or deny based on settings.
                return new BooleanResult() 
                {
                    Success = Settings.Store.AuthzAllowOnError, 
                    Message = "LDAP server unavailable." 
                };
            }

            // If we require authentication, and we failed to auth this user, then we
            // fail authorization.  Note that we do this AFTER checking the LDAP server object
            // because we may want to succeed if the authentication failed due to server
            // being unavailable.
            if (requireAuth)
            {
                PluginActivityInformation actInfo = properties.GetTrackedSingle<PluginActivityInformation>();
                try
                {
                    BooleanResult ldapResult = actInfo.GetAuthenticationResult(this.Uuid);
                    if (!ldapResult.Success)
                    {
                        m_logger.InfoFormat("Deny because LDAP auth failed, and configured to require LDAP auth.");
                        return new BooleanResult()
                        {
                            Success = false,
                            Message = "Deny because LDAP authentication failed."
                        };
                    }
                }
                catch (KeyNotFoundException)
                {
                    // The plugin is not enabled for authentication
                    m_logger.ErrorFormat("LDAP is not enabled for authentication, and authz is configured to require authentication.");
                    return new BooleanResult
                    {
                        Success = false,
                        Message = "Deny because LDAP auth did not execute, and configured to require LDAP auth."
                    };
                }
            }

            // Apply the authorization rules
            try
            {
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
                string user = userInfo.Username;
                
                // Bind for searching if we have rules to process.  If there's only one, it's the
                // default rule which doesn't require searching the LDAP tree.
                if (rules.Count > 1)
                    serv.BindForSearch();

                foreach (GroupAuthzRule rule in rules)
                {
                    bool inGroup = false;
                    
                    // Don't need to check membership if the condition is "always."  This is the
                    // case for the default rule only. which is the last rule in the list.
                    if (rule.RuleCondition != GroupRule.Condition.ALWAYS)
                    {
                        inGroup = serv.MemberOfGroup(user, rule.Group);
                        m_logger.DebugFormat("User {0} {1} member of group {2}", user, inGroup ? "is" : "is not",
                            rule.Group);
                    }

                    if (rule.RuleMatch(inGroup))
                    {
                        if (rule.AllowOnMatch)
                            return new BooleanResult()
                            {
                                Success = true,
                                Message = string.Format("Allow via rule: \"{0}\"", rule.ToString())
                            };
                        else
                            return new BooleanResult()
                            {
                                Success = false,
                                Message = string.Format("Deny via rule: \"{0}\"", rule.ToString())
                            };
                    }
                }

                // We should never get this far because the last rule in the list should always be a match,
                // but if for some reason we do, return success.
                return new BooleanResult() { Success = true, Message = "" };
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
                        serv.Close();
                        properties.AddTrackedSingle<LdapServer>(null);
                        return new BooleanResult 
                        { 
                            Success = Settings.Store.AuthzAllowOnError, 
                            Message = "Failed to contact LDAP server." 
                        };
                    }
                    else if (ldapEx.ErrorCode == 49)
                    {
                        // This is invalid credentials, return false, but server object should remain connected
                        m_logger.ErrorFormat("LDAP bind failed: invalid credentials.");
                        return new BooleanResult 
                        { 
                            Success = false, 
                            Message = "Authorization via LDAP failed. Invalid credentials." 
                        };
                    }
                }

                // Unexpected error, let the PluginDriver catch
                m_logger.ErrorFormat("Error during authorization: {0}", e);
                throw;
            }
        }

        public BooleanResult AuthenticatedUserGateway(SessionProperties properties)
        {
            m_logger.Debug("LDAP Plugin Gateway");
            List<string> addedGroups = new List<string>();

            LdapServer serv = properties.GetTrackedSingle<LdapServer>();

            // If the server is unavailable, we go ahead and succeed anyway.
            if (serv == null)
            {
                m_logger.ErrorFormat("AuthenticatedUserGateway: Internal error, LdapServer object not available.");
                return new BooleanResult() 
                { 
                    Success = true, 
                    Message = "LDAP server not available" 
                };
            }

            try
            {
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
                string user = userInfo.Username;

                List<GroupGatewayRule> rules = GroupRuleLoader.GetGatewayRules();
                bool boundToServ = false;
                foreach (GroupGatewayRule rule in rules)
                {
                    bool inGroup = false;

                    // Don't need to check for group membership if the rule is to be always applied.
                    if (rule.RuleCondition != GroupRule.Condition.ALWAYS)
                    {
                        // If we haven't bound to server yet, do so.
                        if (!boundToServ)
                        {
                            serv.BindForSearch();
                            boundToServ = true;
                        }

                        inGroup = serv.MemberOfGroup(user, rule.Group);
                        m_logger.DebugFormat("User {0} {1} member of group {2}", user, inGroup ? "is" : "is not",
                            rule.Group);
                    }

                    if (rule.RuleMatch(inGroup))
                    {
                        m_logger.InfoFormat("Adding user {0} to local group {1}, due to rule \"{2}\"", 
                            user, rule.LocalGroup, rule.ToString());
                        addedGroups.Add(rule.LocalGroup);
                        userInfo.AddGroup( new GroupInformation() { Name = rule.LocalGroup } );
                    }
                }
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Error during gateway: {0}", e);

                // Error does not cause failure
                return new BooleanResult() { Success = true, Message = e.Message };
            }

            string message = "";
            if (addedGroups.Count > 0)
                message = string.Format("Added to groups: {0}", string.Join(", ", addedGroups));
            else
                message = "No groups added.";

            return new BooleanResult() { Success = true, Message = message };
        }

        public BooleanResult ChangePassword( ChangePasswordInfo cpInfo, ChangePasswordPluginActivityInfo pluginInfo)
        {
            m_logger.Debug("ChangePassword()");

            try
            {
                LdapServer serv = new LdapServer();

                // Authenticate using old password
                BooleanResult result = serv.Authenticate(cpInfo.Username, cpInfo.OldPassword);
                if (!result.Success)
                {
                    return new BooleanResult { Success = false, Message = "Password change failed: Invalid LDAP username or password." };
                }

                // Set the new password
                serv.SetPassword(cpInfo.Username, cpInfo.NewPassword);

                return new BooleanResult { Success = true, Message = "LDAP password successfully changed" };
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Exception in ChangePassword: {0}", e);
                return new BooleanResult() { Success = false, Message = "Error in LDAP plugin." };
            }

        }
    }
}
