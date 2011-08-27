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

using System.DirectoryServices.AccountManagement;

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;

namespace pGina.Plugin.LocalMachine
{
    public class LocalMachineManager : IPluginAuthenticationGateway, IPluginAuthentication
    {
        private ILog m_logger = LogManager.GetLogger("LocalMachineManager");

        public LocalMachineManager()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }
 
        public string Name
        {
            get { return "Local Machine Account Management"; }
        }

        public string Description
        {
            get { return "Manages local machine accounts for authenticated users, and authenticates against the local SAM"; }
        }

        public string Version
        {
            get { return "1.0.0"; }
        }

        public Guid Uuid
        {
            get { return new Guid("{12FA152D-A2E3-4C8D-9535-5DCD49DFCB6D}"); }      // May be used for explicit ordering in the future
        }
        
        public BooleanResult AuthenticatedUserGateway(SessionProperties properties)
        {
            throw new NotImplementedException();
        }

        public BooleanResult AuthenticateUser(SessionProperties properties)
        {
            try
            {                
                m_logger.DebugFormat("AuthenticateUser({0})", properties.Id.ToString());

                // Get user info
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

                m_logger.DebugFormat("Found username: {0}", userInfo.Username);

                using(PrincipalContext pc = new PrincipalContext(ContextType.Machine, Environment.MachineName))
                {
                    if(pc.ValidateCredentials(userInfo.Username, userInfo.Password))
                    {
                        m_logger.InfoFormat("Authenticated user: {0}", userInfo.Username);
                        userInfo.Domain = Environment.MachineName;
                        return new BooleanResult() { Success = true };
                    }
                }                                

                m_logger.ErrorFormat("Failed to authenticate user: {0}", userInfo.Username);
                return new BooleanResult() { Success = false, Message = string.Format("Local account validation failed.") };
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("AuthenticateUser exception: {0}", e);
                throw;  // Allow pGina service to catch and handle exception
            }
        }
    }
}
