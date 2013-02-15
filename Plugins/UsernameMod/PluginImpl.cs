/*
	Copyright (c) 2012, pGina Team
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

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using pGina.Shared.Settings;
using pGina.Plugin.UsernameMod.Rules;

namespace pGina.Plugin.UsernameMod
{
    public class UsernameModPlugin : IPluginConfiguration, IPluginAuthentication, IPluginAuthorization, IPluginAuthenticationGateway
    {
        private ILog m_logger = LogManager.GetLogger("UsernameModPlugin");
        public static Guid SimpleUuid = new Guid("{98477B3A-830D-4BEE-B270-2D7435275F9C}");
        private string m_defaultDescription = "Modify the username at various stages of the login process";
        private dynamic m_settings = null;
        //private ListOfRules rules;


        public UsernameModPlugin()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                m_settings = new pGinaDynamicSettings(SimpleUuid);
                m_settings.SetDefault("ShowDescription", true);
                m_settings.SetDefault("Description", m_defaultDescription);
                
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }        

        public string Name
        {
            get { return "Modify Username Plugin"; }
        }

        public string Description
        {
            get { return m_settings.Description; }
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
            get { return SimpleUuid; }
        }
        
        /// <summary>
        /// Runs the rules against the username, modifying it for future plugins. 
        /// If a IMatchRule is present and matches, it will allow a login for that
        /// user regardless of an entered password. 
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        BooleanResult IPluginAuthentication.AuthenticateUser(SessionProperties properties)
        {
            try
            {
                m_logger.DebugFormat("AuthenticateUser({0})", properties.Id.ToString());

                ListOfRules rules = new ListOfRules();
                rules.Load();

                // Get user info
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
                
                bool authenticated = false; //By default, we don't authenticate
                string username = userInfo.Username;

                m_logger.DebugFormat("Start of Authentication, username: {0}", username);

                foreach (IUsernameRule rule in rules.list)
                {
                    
                    if(rule.stage == Stage.Authentication){
                        m_logger.DebugFormat("[Authentication] Running rule: {0}", rule.ToString());
                        if (rule is IModifyRule)
                        {
                            IModifyRule mRule = (IModifyRule)rule;
                            username = mRule.modify(username);
                            m_logger.DebugFormat("Username modified: {0}", username);
                        } else if(rule is IMatchRule){
                            authenticated = ((IMatchRule)rule).match(username) ? true : authenticated;
                            m_logger.DebugFormat("Authenticated? {0}", authenticated);
                        }
                    }
                }

                //Set the changes to the username
                userInfo.Username = username;

                return new BooleanResult() { Success = authenticated };                
            }

            catch (Exception e)
            {
                m_logger.ErrorFormat("Error running rules. {0}", e.Message);
                return new BooleanResult() { Success = false, Message = "Unable to modify username during authentication stage." };                
            }
        }

        /// <summary>
        /// Runs the rules against the username, modifying it for future plugins.
        /// If an IMatchRule is present, AuthorizeUser will only return true if
        /// the username matches the rule.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        BooleanResult IPluginAuthorization.AuthorizeUser(SessionProperties properties)
        {
            try
            {
                m_logger.DebugFormat("AuthorizeUser({0})", properties.Id.ToString());

                ListOfRules rules = new ListOfRules();
                rules.Load();

                // Get user info
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

                bool authorized = true; //By default, we don't authenticate
                string username = userInfo.Username;

                m_logger.DebugFormat("Start of Authorization, username: {0}", username);

                foreach (IUsernameRule rule in rules.list)
                {
                    if (rule.stage == Stage.Authorization)
                    {
                        m_logger.DebugFormat("[Authorization] Checking rule: {0}", rule.ToString());
                        if (rule is IModifyRule)
                        {
                            IModifyRule mRule = (IModifyRule)rule;
                            username = mRule.modify(username);
                            m_logger.DebugFormat("Username modified: {0}", username);
                        }
                        else if (rule is IMatchRule)
                        {
                            //If the match rule fails, do not authorize
                            authorized = ((IMatchRule)rule).match(username) ? authorized : false;
                            m_logger.DebugFormat("Authorized? {0}", authorized);
                        }
                    }
                }

                //Set the changes to the username
                userInfo.Username = username;

                return new BooleanResult() { Success = authorized };
            }

            catch (Exception e)
            {
                m_logger.ErrorFormat("Error running rules. {0}", e.Message);
                return new BooleanResult() { Success = false, Message = "Unable to modify username during authorization stage." };
            }
        }

        /// <summary>
        /// Runs the rules against the username, modifying it for future plugins.
        /// If an IMatchRule is present, AuthenticatedUserGateway will only return true if
        /// the username matches the rule.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        BooleanResult IPluginAuthenticationGateway.AuthenticatedUserGateway(SessionProperties properties)
        {
            try
            {
                m_logger.DebugFormat("GatewayUser({0})", properties.Id.ToString());

                ListOfRules rules = new ListOfRules();
                rules.Load();

                // Get user info
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

                bool authGateway = true; //By default, we don't authenticate
                string username = userInfo.Username;

                m_logger.DebugFormat("Start of Gateway, username: {0}", username);

                foreach (IUsernameRule rule in rules.list)
                {
                    if (rule.stage == Stage.Gateway)
                    {
                        m_logger.DebugFormat("[Gateway] Checking rule: {0}", rule.ToString());
                        if (rule is IModifyRule)
                        {
                            IModifyRule mRule = (IModifyRule)rule;
                            username = mRule.modify(username);
                            m_logger.DebugFormat("Username modified: {0}", username);
                        }
                        else if (rule is IMatchRule)
                        {
                            //If the match rule fails, do not authorize
                            authGateway = ((IMatchRule)rule).match(username) ? authGateway : false;
                            m_logger.DebugFormat("Auth'd Gateway? {0}", authGateway);
                        }
                    }
                }

                //Set the changes to the username
                userInfo.Username = username;

                return new BooleanResult() { Success = authGateway };
            }

            catch (Exception e)
            {
                m_logger.ErrorFormat("Error running rules. {0}", e.Message);
                return new BooleanResult() { Success = false, Message = "Unable to modify username during gateway stage." };
            }
        }
        
        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }

        public void Starting() { }
        public void Stopping() { }
    }

    class UsernameModPluginException : Exception
    {
        public UsernameModPluginException(string message)
            : base(message)
        {

        }

        public UsernameModPluginException(string message, Exception inner)
            : base(message)
        {

        }
    }
}
