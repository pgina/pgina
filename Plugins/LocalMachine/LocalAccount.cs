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
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;

using pGina.Shared.Types;

namespace pGina.Plugin.LocalMachine
{
    class LocalAccount
    {
        private static ILog m_logger = LogManager.GetLogger("LocalAccount");
        private static DirectoryEntry m_sam = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");

        public class GroupSyncException : Exception 
        {
            public GroupSyncException(Exception e)
            {
                RootException = e;
            }

            public Exception RootException { get; private set; }
        };

        public static DirectoryEntry GetUserEntry(string username)
        {
            try
            {
                return m_sam.Children.Find(username, "User");
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("GetUserEntry({0}) failed: {1}", username, e);
                return null;
            }
        }

        public static DirectoryEntry GetGroupEntry(string groupName)
        {
            try
            {
                return m_sam.Children.Find(groupName, "Group");
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("GetGroupEntry({0}) failed: {1}", groupName, e);
                return null;
            }
        }

        public static bool IsUserInGroup(DirectoryEntry user, DirectoryEntry group)
        {
            try
            {                                             
                foreach (object member in (IEnumerable<object>) group.Invoke("Member", null))
                {                 
                    using (DirectoryEntry memberEntry = new DirectoryEntry(member))
                    {
                        if (user.Path == memberEntry.Path)
                            return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }                
        }

        public static DirectoryEntry CreateOrGetGroup(string groupname)
        {
            DirectoryEntry group = GetGroupEntry(groupname);
            if (group == null)
            {
                group = m_sam.Children.Add(groupname, "Group");
                m_sam.CommitChanges();
            }

            return group;
        }

        public static DirectoryEntry CreateOrGetUser(pGina.Shared.Types.UserInformation userInfo)
        {
            // Get the user's object, if we can
            DirectoryEntry user = GetUserEntry(userInfo.Username);

            // If non-existent, add it!
            if (user == null)
            {
                user = m_sam.Children.Add(userInfo.Username, "User");
                m_sam.CommitChanges();                
            }

            return user;
        }

        public static void AddUserToGroup(DirectoryEntry user, GroupInformation groupInfo)
        {
            using (DirectoryEntry group = CreateOrGetGroup(groupInfo.Name))
            {
                string existingDescription = (string)group.Properties["Description"].Value;
                if (!string.IsNullOrEmpty(groupInfo.Description) && existingDescription != groupInfo.Description)
                {
                    group.Properties["Description"].Value = groupInfo.Description;                    
                    group.CommitChanges();
                }

                if (!IsUserInGroup(user, group))
                {
                    group.Invoke("Add", new object[] { user.Path.ToString() });
                    group.CommitChanges();
                }
            }
        }        

        public static void SyncUserInfoToLocalUser(UserInformation userInfo)
        {
            using (DirectoryEntry user = CreateOrGetUser(userInfo))
            {
                string fullname = userInfo.Fullname;
                if (string.IsNullOrEmpty(fullname))
                    fullname = string.Format("{0} (pGina)", userInfo.Username);

                if(string.IsNullOrEmpty((string) user.Properties["FullName"].Value))
                    user.Properties["FullName"].Value = fullname;                

                user.Invoke("SetPassword", new object[] { userInfo.Password });
                user.CommitChanges();

                // Sync groups
                try
                {
                    foreach (GroupInformation group in userInfo.Groups)
                    {
                        AddUserToGroup(user, group);
                    }

                    // Always add to Users
                    AddUserToGroup(user, new GroupInformation() { Name = "Users" });
                }
                catch(Exception e)
                {
                    throw new GroupSyncException(e);
                }
            }     
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

        // Load userInfo.Username's group list and populate userInfo.Groups accordingly
        public static void SyncLocalUserGroupsToUserInfo(UserInformation userInfo)
        {
            SecurityIdentifier EveryoneSid = new SecurityIdentifier("S-1-1-0");
            SecurityIdentifier AuthenticatedUsersSid = new SecurityIdentifier("S-1-5-11");

            using (PrincipalContext pc = new PrincipalContext(ContextType.Machine, Environment.MachineName))
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(pc, IdentityType.Name, userInfo.Username))
                {
                    if (user != null)
                    {
                        foreach (GroupPrincipal group in user.GetAuthorizationGroups())
                        {
                            // Skip "Authenticated Users" and "Everyone" as these are generated
                            if (group.Sid == EveryoneSid || group.Sid == AuthenticatedUsersSid)
                                continue;

                            userInfo.AddGroup(new GroupInformation()
                            {
                                Name = group.Name,
                                Description = group.Description,
                                SID = group.Sid
                            });
                        }
                    }
                }
            }
        }
    }
}
