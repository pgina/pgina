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

        public PluginDriver()
        {
            m_logger = LogManager.GetLogger(string.Format("PluginDriver:{0}", m_sessionId));            

            m_properties = new SessionProperties(m_sessionId);

            // Add the user information object we'll be using for this session
            UserInformation userInfo = new UserInformation();
            m_properties.AddTrackedSingle<UserInformation>(userInfo);
            
            // Add the plugin tracking object we'll be using for this session
            PluginActivityInformation pluginInfo = new PluginActivityInformation();
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

        public BooleanResult PerformLoginProcess()
        {
            try
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
            catch (Exception e)
            {
                // We catch exceptions at a high level here and report failure in these cases,
                //  with the exception details as our message for now
                m_logger.ErrorFormat("Exception during login process: {0}", e);
                return new BooleanResult() { Success = false, Message = string.Format("Exception occurred: {0}", e) };
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
                        m_logger.WarnFormat("{0} Failed with Message: {1}", plugin.Uuid, pluginResult.Message);
                        finalResult.Message = pluginResult.Message;
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
