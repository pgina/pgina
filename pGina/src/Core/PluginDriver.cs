using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;

namespace pGina.Core
{
    public class PluginDriver
    {
        private Guid m_sessionId = Guid.NewGuid();
        private SessionProperties m_properties = null;        

        public PluginDriver()
        {
            m_properties = new SessionProperties(m_sessionId);
            UserInformation userInfo = new UserInformation();
            m_properties.AddTrackedSingle<UserInformation>(userInfo);
            PluginActivityInformation pluginInfo = new PluginActivityInformation();
            m_properties.AddTrackedSingle<PluginActivityInformation>(pluginInfo);
        }

        public string Username
        {
            get
            {
                return m_properties.GetTrackedSingle<UserInformation>().Username;
            }

            set
            {
                m_properties.GetTrackedSingle<UserInformation>().Username = value;
            }
        }

        public string Password
        {
            get
            {
                return m_properties.GetTrackedSingle<UserInformation>().Password;
            }

            set
            {
                m_properties.GetTrackedSingle<UserInformation>().Password = value;
            }
        }

        public BooleanResult AuthenticateUser()
        {
            PluginActivityInformation pluginInfo = m_properties.GetTrackedSingle<PluginActivityInformation>();
            List<IPluginAuthentication> plugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthentication>();

            // At least one must succeed
            BooleanResult finalResult = new BooleanResult() { Success = false };

            foreach (IPluginAuthentication plugin in plugins)
            {
                BooleanResult pluginResult = plugin.AuthenticateUser(m_properties);
                pluginInfo.AddAuthenticateResult(plugin.Uuid, pluginResult);
                if (pluginResult.Success)
                    finalResult.Success = true;
                else
                    finalResult.Message = pluginResult.Message;                
            }

            if(finalResult.Success) { finalResult.Message = null; } // Clear any errors from plugins if we did succeed
            return finalResult;
        }

        public BooleanResult AuthorizeUser()
        {
            PluginActivityInformation pluginInfo = m_properties.GetTrackedSingle<PluginActivityInformation>();
            List<IPluginAuthorization> plugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthorization>();

            foreach (IPluginAuthorization plugin in plugins)
            {
                BooleanResult pluginResult = plugin.AuthorizeUser(m_properties);
                pluginInfo.AddAuthorizationResult(plugin.Uuid, pluginResult);

                // All must succeed, fail = total fail
                if (!pluginResult.Success)
                    return pluginResult;
            }

            return new BooleanResult() { Success = true };
        }

        public BooleanResult GatewayProcess()
        {
            PluginActivityInformation pluginInfo = m_properties.GetTrackedSingle<PluginActivityInformation>();
            List<IPluginAuthenticationGateway> plugins = PluginLoader.GetOrderedPluginsOfType<IPluginAuthenticationGateway>();

            foreach (IPluginAuthenticationGateway plugin in plugins)
            {
                BooleanResult pluginResult = plugin.AuthenticatedUserGateway(m_properties);
                pluginInfo.AddGatewayResult(plugin.Uuid, pluginResult);

                // All must succeed, fail = total fail
                if (!pluginResult.Success)
                    return pluginResult;
            }

            return new BooleanResult() { Success = true };
        }
    }
}
