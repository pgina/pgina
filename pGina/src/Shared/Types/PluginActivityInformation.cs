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

namespace pGina.Shared.Types
{    
    public class PluginActivityInformation
    {        
        private Dictionary<Guid, BooleanResult> m_authentication = new Dictionary<Guid, BooleanResult>();
        private Dictionary<Guid, BooleanResult> m_authorization = new Dictionary<Guid, BooleanResult>();
        private Dictionary<Guid, BooleanResult> m_gateway = new Dictionary<Guid, BooleanResult>();

        public List<Interfaces.IPluginAuthentication> LoadedAuthenticationPlugins { get; set; }
        public List<Interfaces.IPluginAuthenticationGateway> LoadedAuthenticationGatewayPlugins { get; set; }
        public List<Interfaces.IPluginAuthorization> LoadedAuthorizationPlugins { get; set; }        

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

        public IEnumerable<Guid> GetAuthenticationPlugins()
        {
            foreach (KeyValuePair<Guid, BooleanResult> kv in m_authentication)
            {
                yield return kv.Key;
            }
        }

        public IEnumerable<Guid> GetAuthorizationPlugins()
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
