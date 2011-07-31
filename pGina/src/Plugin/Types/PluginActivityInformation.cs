using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Shared.Types
{    
    public class PluginActivityInformation
    {        
        private Dictionary<Guid, BooleanResult> m_authentication = new Dictionary<Guid, BooleanResult>();
        private Dictionary<Guid, BooleanResult> m_authorization = new Dictionary<Guid, BooleanResult>();
        private Dictionary<Guid, BooleanResult> m_gateway = new Dictionary<Guid, BooleanResult>();

        public void AddAuthenticateResult(Guid pluginId, BooleanResult result)
        {
            if(m_authentication.ContainsKey(pluginId))
                m_authentication[pluginId] = result;
            else
                m_authentication.Add(pluginId, result);
        }

        public void AddAuthorizationResult(Guid pluginId, BooleanResult result)
        {
            if(m_authorization.ContainsKey(pluginId))
                m_authorization[pluginId] = result;
            else
                m_authorization.Add(pluginId, result);
        }

        public void AddGatewayResult(Guid pluginId, BooleanResult result)
        {
            if (m_gateway.ContainsKey(pluginId))
                m_gateway[pluginId] = result;
            else
                m_gateway.Add(pluginId, result);
        }


        public BooleanResult GetAuthenticationResult(Guid pluginGuid)
        {
            return m_authentication[pluginGuid];
        }

        public BooleanResult GetAuthorizationResult(Guid pluginGuid)
        {
            return m_authorization[pluginGuid];
        }

        public BooleanResult GetGatewayResult(Guid pluginGuid)
        {
            return m_gateway[pluginGuid];
        }

        public IEnumerable<Guid> GetAuthenticatedPlugins()
        {
            foreach (KeyValuePair<Guid, BooleanResult> kv in m_authentication)
            {
                yield return kv.Key;
            }
        }

        public IEnumerable<Guid> GetAuthorizedPlugins()
        {
            foreach (KeyValuePair<Guid, BooleanResult> kv in m_authorization)
            {
                yield return kv.Key;
            }
        }

        public IEnumerable<Guid> GetGatewayPlugins()
        {
            foreach (KeyValuePair<Guid, BooleanResult> kv in m_gateway)
            {
                yield return kv.Key;
            }
        }
    }
}
