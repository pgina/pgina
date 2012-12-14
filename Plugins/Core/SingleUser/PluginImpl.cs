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
using System.Diagnostics;

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using pGina.Shared.Settings;

namespace pGina.Plugin.SingleUser
{
    public class PluginImpl : IPluginConfiguration, IPluginAuthenticationGateway
    {
        private ILog m_logger = LogManager.GetLogger("SingleUserPlugin");
        public static Guid PluginUuid = new Guid("{81F8034E-E278-4754-B10C-7066656DE5B7}");        
        
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
            get { return "Single User Login"; }
        }

        public string Description
        {
            get { return "Allow re-direction of all authenticated users to a single set of credentials"; }
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

            string username = Settings.Store.Username;
            string domain = Settings.Store.Domain;
            string password = Settings.Store.GetEncryptedSetting("Password", null);
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

            // Substitute
            m_logger.DebugFormat("Re-writing user information to login with {0}\\{1}", domain, username);
            properties.GetTrackedSingle<UserInformation>().Username = username;
            properties.GetTrackedSingle<UserInformation>().Domain = domain;
            properties.GetTrackedSingle<UserInformation>().Password = password;
            return new BooleanResult() { Success = true };
        }

        public void Starting() { }
        public void Stopping() { }
    }
}
