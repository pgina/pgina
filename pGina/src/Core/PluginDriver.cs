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

namespace pGina.Core
{
    public class PluginDriver
    {
        private Guid m_sessionId = Guid.NewGuid();
        private SessionProperties m_properties = null;
        private ILog m_logger = null;

        public Guid SessionId
        {
            get { return m_sessionId; }
            set 
            { 
                m_sessionId = value;
                m_properties.Id = value;
                m_properties.AddTrackedObject("SessionId", new Guid(m_sessionId.ToString()));
            }
        }

        public PluginDriver()
        {
            m_logger = LogManager.GetLogger(string.Format("PluginDriver:{0}", m_sessionId));            

            m_properties = new SessionProperties(m_sessionId);

            // Add the user information object we'll be using for this session
            UserInformation userInfo = new UserInformation();
            m_properties.AddTrackedSingle<UserInformation>(userInfo);
            
            // Add the plugin tracking object we'll be using for this session
            PluginActivityInformation pluginInfo = new PluginActivityInformation();
            pluginInfo.LoadedAuthenticationGatewayPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthenticationGateway>();
            pluginInfo.LoadedAuthenticationPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthentication>();
            pluginInfo.LoadedAuthorizationPlugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthorization>();
            m_properties.AddTrackedSingle<PluginActivityInformation>(pluginInfo);

            m_logger.DebugFormat("New PluginDriver created");
        }

        public UserInformation UserInformation
        {
            get { return m_properties.GetTrackedSingle<UserInformation>(); }
        }

        public List<GroupInformation> GroupInformation
        {
            get { return m_properties.GetTrackedSingle<UserInformation>().Groups; }
        }

        public SessionProperties SessionProperties
        {
            get { return m_properties; }
        }

        public static void Starting()
        {
            foreach (IPluginBase plugin in PluginLoader.AllPlugins)
                plugin.Starting();
        }

        public static void Stopping()
        {
            foreach (IPluginBase plugin in PluginLoader.AllPlugins)
                plugin.Stopping();
        }

        public BooleanResult PerformLoginProcess()
        {
            try
            {
                // Set the original username to the current username if not already set
                UserInformation userInfo = m_properties.GetTrackedSingle<UserInformation>();
                if (string.IsNullOrEmpty(userInfo.OriginalUsername))
                    userInfo.OriginalUsername = userInfo.Username;

                // Execute login
                BeginChain();
                BooleanResult result = ExecuteLoginChain();
                EndChain();

                return result;
            }
            catch (Exception e)
            {
                // We catch exceptions at a high level here and report failure in these cases,
                //  with the exception details as our message for now
                m_logger.ErrorFormat("Exception during login process: {0}", e);
                return new BooleanResult() { Success = false, Message = string.Format("Unhandled exception during login: {0}", e) };
            }
        }

        private BooleanResult ExecuteLoginChain()
        {
            m_logger.DebugFormat("Performing login process");
            BooleanResult result = AuthenticateUser();
            if (!result.Success)
                return result;

            result = AuthorizeUser();
            if (!result.Success)
                return result;

            result = GatewayProcess();                
            return result;
        }

        public void BeginChain()
        {
            List<IStatefulPlugin> plugins = PluginLoader.GetEnabledStatefulPlugins();
            m_logger.DebugFormat("Begin login chain, {0} stateful plugin(s).", plugins.Count);
            foreach (IStatefulPlugin plugin in plugins)
            {
                plugin.BeginChain(m_properties);
            }
        }

        public void EndChain()
        {
            List<IStatefulPlugin> plugins = PluginLoader.GetEnabledStatefulPlugins();
            m_logger.DebugFormat("End login chain, {0} stateful plugin(s).", plugins.Count);
            foreach (IStatefulPlugin plugin in plugins)
            {
                plugin.EndChain(m_properties);
            }
        }

        public BooleanResult AuthenticateUser()
        {            
            PluginActivityInformation pluginInfo = m_properties.GetTrackedSingle<PluginActivityInformation>();
            List<IPluginAuthentication> plugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthentication>();

            m_logger.DebugFormat("Authenticating user {0}, {1} plugins available", m_properties.GetTrackedSingle<UserInformation>().Username, plugins.Count);

            // At least one must succeed
            BooleanResult finalResult = new BooleanResult() { Success = false };

            foreach (IPluginAuthentication plugin in plugins)
            {
                m_logger.DebugFormat("Calling {0}", plugin.Uuid);

                BooleanResult pluginResult = new BooleanResult() { Message = null, Success = false };

                try
                {
                    pluginResult = plugin.AuthenticateUser(m_properties);
                    pluginInfo.AddAuthenticateResult(plugin.Uuid, pluginResult);

                    if (pluginResult.Success)
                    {
                        m_logger.DebugFormat("{0} Succeeded", plugin.Uuid);
                        finalResult.Success = true;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(pluginResult.Message))
                        {
                            m_logger.WarnFormat("{0} Failed with Message: {1}", plugin.Uuid, pluginResult.Message);
                            finalResult.Message = pluginResult.Message;
                        }
                        else
                        {
                            m_logger.WarnFormat("{0} Failed without a message", plugin.Uuid);
                        }
                    }
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("{0} Threw an unexpected exception, assuming failure: {1}", plugin.Uuid, e);
                }
            }

            if (finalResult.Success)
            {
                // Clear any errors from plugins if we did succeed
                finalResult.Message = null;
                m_logger.InfoFormat("Successfully authenticated {0}", m_properties.GetTrackedSingle<UserInformation>().Username);
            }
            else
            {
                m_logger.ErrorFormat("Failed to authenticate {0}, Message: {1}", m_properties.GetTrackedSingle<UserInformation>().Username, finalResult.Message);
            }
            
            return finalResult;
        }

        public BooleanResult AuthorizeUser()
        {
            PluginActivityInformation pluginInfo = m_properties.GetTrackedSingle<PluginActivityInformation>();
            List<IPluginAuthorization> plugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthorization>();
            
            m_logger.DebugFormat("Authorizing user {0}, {1} plugins available", m_properties.GetTrackedSingle<UserInformation>().Username, plugins.Count);

            foreach (IPluginAuthorization plugin in plugins)
            {
                m_logger.DebugFormat("Calling {0}", plugin.Uuid);

                BooleanResult pluginResult = new BooleanResult() { Message = null, Success = false };

                try
                {
                    pluginResult = plugin.AuthorizeUser(m_properties);
                    pluginInfo.AddAuthorizationResult(plugin.Uuid, pluginResult);

                    // All must succeed, fail = total fail
                    if (!pluginResult.Success)
                    {
                        m_logger.ErrorFormat("{0} Failed to authorize {1} message: {2}", plugin.Uuid, m_properties.GetTrackedSingle<UserInformation>().Username, pluginResult.Message);
                        return pluginResult;
                    }
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("{0} Threw an unexpected exception, treating this as failure: {1}", plugin.Uuid, e);
                    return pluginResult;
                }
            }

            m_logger.InfoFormat("Successfully authorized {0}", m_properties.GetTrackedSingle<UserInformation>().Username);
            return new BooleanResult() { Success = true };
        }

        public BooleanResult GatewayProcess()
        {
            PluginActivityInformation pluginInfo = m_properties.GetTrackedSingle<PluginActivityInformation>();
            List<IPluginAuthenticationGateway> plugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthenticationGateway>();

            m_logger.DebugFormat("Processing gateways for user {0}, {1} plugins available", m_properties.GetTrackedSingle<UserInformation>().Username, plugins.Count);

            foreach (IPluginAuthenticationGateway plugin in plugins)
            {
                m_logger.DebugFormat("Calling {0}", plugin.Uuid);

                BooleanResult pluginResult = new BooleanResult() { Message = null, Success = false };

                try
                {
                    pluginResult = plugin.AuthenticatedUserGateway(m_properties);
                    pluginInfo.AddGatewayResult(plugin.Uuid, pluginResult);

                    // All must succeed, fail = total fail
                    if (!pluginResult.Success)
                    {
                        m_logger.ErrorFormat("{0} Failed to process gateway for {1} message: {2}", plugin.Uuid, m_properties.GetTrackedSingle<UserInformation>().Username, pluginResult.Message);
                        return pluginResult;
                    }
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("{0} Threw an unexpected exception, treating this as failure: {1}", plugin.Uuid, e);
                    return pluginResult;
                }
            }

            m_logger.InfoFormat("Successfully processed gateways for {0}", m_properties.GetTrackedSingle<UserInformation>().Username);
            return new BooleanResult() { Success = true };
        }                    
    }
}
