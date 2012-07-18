/*
	Copyright (c) 2011, pGina Team
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
    public class PluginImpl : IPluginAuthentication, IPluginAuthenticationGateway, IPluginConfiguration
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
            get { return "Authenticates users using a MySQL server as the account database."; }
        }

        public string Name
        {
            get { return "MySQL Authentication"; }
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
            catch (Exception e)
            {
                m_logger.ErrorFormat("Unexpected error: {0}", e);
                throw;
            }
            
            // Always return success
            return new Shared.Types.BooleanResult { Success = true };
        }
    }
}
