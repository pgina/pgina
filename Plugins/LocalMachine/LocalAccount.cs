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
using System.DirectoryServices;

namespace pGina.Plugin.LocalMachine
{
    class LocalAccount
    {
        static ILog m_logger = LogManager.GetLogger("LocalAccount");

        public static void Create(pGina.Shared.Types.UserInformation userInfo)
        {
            // Local machine
            using (DirectoryEntry local = new DirectoryEntry("WinNT://localhost"))
            {
                bool userExists = false;

                foreach (DirectoryEntry ent in local.Children)
                {
                    userExists = ent.Name.Equals(userInfo.Username, StringComparison.CurrentCultureIgnoreCase);
                    if (userExists)
                        break;
                }

                if (!userExists)
                {
                    m_logger.InfoFormat("Creating local user {0}", userInfo.Username);
                    using (DirectoryEntry user = local.Children.Add(userInfo.Username, "User"))
                    {
                        user.Properties["FullName"].Add(String.Format("{0} (pGina)", userInfo.Username));
                        user.Invoke("SetPassword", userInfo.Password);
                        // Possibly set some flags here...
                        // user.Invoke("Put", new object[] {"UserFlags", ...});
                        user.CommitChanges();
                    }
                }
                else
                {
                    m_logger.InfoFormat("Account {0} already exists, skipping.", userInfo.Username);
                }
            }

            // TODO: create/load local profile?
        }

        public static void Delete(string userName)
        {
            m_logger.DebugFormat("Deleting user {0}", userName);
            using( DirectoryEntry local = new DirectoryEntry("WinNT://localhost"))
            {
                using (DirectoryEntry user = local.Children.Find(userName))
                {
                    if (user != null)
                    {
                        local.Children.Remove(user);
                    }
                }
            }

            // TODO: Delete local profile?
        }
    }
}
