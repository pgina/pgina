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
using System.Linq;
using System.Text;
using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;

using MySql.Data.MySqlClient;

namespace pGina.Plugin.MySQLAuth
{
    public class PluginImpl : IPluginAuthentication, IPluginAuthorization, IPluginAuthenticationGateway, IPluginConfiguration
    {
        public static readonly Guid PluginUuid = new Guid("{A89DF410-53CA-4FE1-A6CA-4479B841CA19}");
        private ILog m_logger = LogManager.GetLogger("MySQLAuth");

        public Shared.Types.BooleanResult AuthenticateUser(Shared.Types.SessionProperties properties)
        {
            Shared.Types.UserInformation userInfo = properties.GetTrackedSingle<Shared.Types.UserInformation>();

            m_logger.DebugFormat("Authenticate: {0}", userInfo.Username);

            UserEntry entry = null;
            try
            {
                using (MySqlUserDataSource dataSource = new MySqlUserDataSource())
                {
                    entry = dataSource.GetUserEntry(userInfo.Username);
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1042)
                    m_logger.ErrorFormat("Unable to connect to host: {0}", Settings.Store.Host);
                else
                {
                    m_logger.ErrorFormat("{0}", ex);
                    throw;
                }
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Unexpected error: {0}", e);
                throw;
            }
            
            if (entry != null)
            {
                m_logger.DebugFormat("Retrieved info for user {0} from MySQL.  Password uses {1}.",
                    entry.Name, entry.HashAlg.ToString());

                bool passwordOk = entry.VerifyPassword(userInfo.Password);
                if (passwordOk)
                {
                    m_logger.DebugFormat("Authentication successful for {0}", userInfo.Username);
                    return new Shared.Types.BooleanResult() { Success = true, Message = "Success." };
                }
                else
                {
                    m_logger.DebugFormat("Authentication failed for {0}", userInfo.Username); 
                    return new Shared.Types.BooleanResult() { Success = false, Message = "Invalid username or password." };
                }
            }
            else
            {
                m_logger.DebugFormat("Authentication failed for {0}", userInfo.Username);
                return new Shared.Types.BooleanResult() { Success = false, Message = "Invalid username or password." };
            }
        }

        public string Description
        {
            get { return "Uses a MySQL server as the account database."; }
        }

        public string Name
        {
            get { return "MySQL"; }
        }

        public void Starting()
        {
            
        }

        public void Stopping()
        {
            
        }

        public Guid Uuid
        {
            get { return PluginUuid; }
        }

        public string Version
        {
            get 
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public void Configure()
        {
            Configuration dialog = new Configuration();
            dialog.ShowDialog();
        }

        public Shared.Types.BooleanResult AuthenticatedUserGateway(Shared.Types.SessionProperties properties)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            try
            {
                using (MySqlUserDataSource dataSource = new MySqlUserDataSource())
                {
                    List<GroupGatewayRule> rules = GroupRuleLoader.GetGatewayRules();

                    foreach (GroupGatewayRule rule in rules)
                    {
                        m_logger.DebugFormat("Checking rule: {0}", rule.ToString());
                        if (rule.RuleMatch(dataSource.IsMemberOfGroup(userInfo.Username, rule.Group)))
                        {
                            m_logger.DebugFormat("Rule is a match, adding to {0}", rule.LocalGroup);
                            userInfo.Groups.Add(new GroupInformation { Name = rule.LocalGroup });
                        }
                        else
                        {
                            m_logger.DebugFormat("Rule is not a match");
                        }
                    }
                }
            }
            catch(MySqlException e)
            {
                bool preventLogon = Settings.Store.PreventLogonOnServerError;
                if( preventLogon )
                {
                    m_logger.DebugFormat("Encountered MySQL server error, and preventing logon: {0}", e.Message);
                    return new BooleanResult {
                        Success = false,
                        Message = string.Format("Preventing logon due to server error: {0}", e.Message)
                    };
                } 
                else 
                {
                    m_logger.DebugFormat("Encoutered MySQL server error, but returning success anyway.  Error: {0}", e.Message);
                    return new BooleanResult {
                        Success = true,
                        Message = string.Format("Encountered server error: {0}", e.Message)
                    };
                }
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Unexpected error: {0}", e);
                throw;
            }
            
            // Always return success
            return new Shared.Types.BooleanResult { Success = true };
        }

        public BooleanResult AuthorizeUser(SessionProperties properties)
        {
            m_logger.Debug("MySql Plugin Authorization");

            bool requireAuth = Settings.Store.AuthzRequireMySqlAuth;
            
            // If we require authentication, and we failed to auth this user, then we
            // fail authorization.
            if (requireAuth)
            {
                PluginActivityInformation actInfo = properties.GetTrackedSingle<PluginActivityInformation>();
                try
                {
                    BooleanResult mySqlResult = actInfo.GetAuthenticationResult(this.Uuid);
                    if (!mySqlResult.Success)
                    {
                        m_logger.InfoFormat("Deny because MySQL auth failed, and configured to require MySQL auth.");
                        return new BooleanResult()
                        {
                            Success = false,
                            Message = "Deny because MySQL authentication failed."
                        };
                    }
                }
                catch (KeyNotFoundException)
                {
                    // The plugin is not enabled for authentication
                    m_logger.ErrorFormat("MySQL is not enabled for authentication, and authz is configured to require auth.");
                    return new BooleanResult
                    {
                        Success = false,
                        Message = "Deny because MySQL auth did not execute, and configured to require MySQL auth."
                    };
                }
            }
            
            // Get the authz rules from registry
            List<GroupAuthzRule> rules = GroupRuleLoader.GetAuthzRules();
            if (rules.Count == 0)
            {
                throw new Exception("No authorization rules found.");
            }

            try
            {
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
                string user = userInfo.Username;

                using (MySqlUserDataSource dataSource = new MySqlUserDataSource())
                {
                    foreach (GroupAuthzRule rule in rules)
                    {
                        m_logger.DebugFormat("Checking rule: {0}", rule.ToString());
                        bool inGroup = false;

                        if (rule.RuleCondition != GroupRule.Condition.ALWAYS)
                        {
                            inGroup = dataSource.IsMemberOfGroup(user, rule.Group);
                            m_logger.DebugFormat("User '{0}' {1} a member of '{2}'", user,
                                inGroup ? "is" : "is not", rule.Group);
                        }

                        if (rule.RuleMatch(inGroup))
                        {
                            if (rule.AllowOnMatch)
                                return new BooleanResult
                                {
                                    Success = true,
                                    Message = string.Format("Allow via rule '{0}'", rule.ToString() )
                                };
                            else
                                return new BooleanResult
                                {
                                    Success = false,
                                    Message = string.Format("Deny via rule '{0}'", rule.ToString())
                                };
                        }
                    }
                }

                // If we get this far, no rules matched.  This should never happen since
                // the last rule should always match (the default).  Throw.
                throw new Exception("Missing default authorization rule.");
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Exception during authorization: {0}", e);
                throw;
            }
        }
    }
}
