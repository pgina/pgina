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
    public class LocalAccount
    {
        private static ILog m_logger = null; 
        private static DirectoryEntry m_sam = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
        private static PrincipalContext m_machinePrincipal = new PrincipalContext(ContextType.Machine, Environment.MachineName);
        
        public class GroupSyncException : Exception 
        {
            public GroupSyncException(Exception e)
            {
                RootException = e;
            }

            public Exception RootException { get; private set; }
        };

        private UserInformation m_userInfo = null;
        public UserInformation UserInfo 
        {
            get { return m_userInfo; }
            set
            {
                m_userInfo = value;
                m_logger = LogManager.GetLogger(string.Format("LocalAccount[{0}]", m_userInfo.Username));
            }
        }
        
        public LocalAccount()
        {
            m_logger = LogManager.GetLogger("LocalAccount");
        }

        public LocalAccount(UserInformation userInfo)
        {
            UserInfo = userInfo;            
        }        

        public static UserPrincipal GetUserPrincipal(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            return UserPrincipal.FindByIdentity(m_machinePrincipal, IdentityType.Name, username);
        }

        public static UserPrincipal GetUserPrincipal(SecurityIdentifier sid)
        {
            return UserPrincipal.FindByIdentity(m_machinePrincipal, IdentityType.Sid, sid.ToString());
        }

        public static GroupPrincipal GetGroupPrincipal(string groupname)
        {
            if (string.IsNullOrEmpty(groupname)) return null;
            return GroupPrincipal.FindByIdentity(m_machinePrincipal, IdentityType.Name, groupname);
        }

        public static GroupPrincipal GetGroupPrincipal(SecurityIdentifier sid)
        {
            return GroupPrincipal.FindByIdentity(m_machinePrincipal, IdentityType.Sid, sid.ToString());
        }

        public static DirectoryEntry GetUserDirectoryEntry(string username)
        {
            return m_sam.Children.Find(username, "User");
        }

        public static void ScrambleUsersPassword(string username)
        {
            using (DirectoryEntry userDe = GetUserDirectoryEntry(username))
            {
                userDe.Invoke("SetPassword", new object[] { Guid.NewGuid().ToString().Replace("-", "").Replace("{", "").Replace("}", "") });
                userDe.CommitChanges();
            }
        } 
        
        // Non recursive group check (immediate membership only currently)
        private bool IsUserInGroup(string username, string groupname)
        {
            using(GroupPrincipal group = GetGroupPrincipal(groupname))
            {
                if (group == null) return false;

                using(UserPrincipal user = GetUserPrincipal(username))
                {
                    if (user == null) return false;

                    return IsUserInGroup(user, group);
                }                
            }            
        }

        // Non recursive group check (immediate membership only currently)
        private bool IsUserInGroup(UserPrincipal user, GroupPrincipal group)
        {
            if (user == null || group == null) return false;

            foreach (Principal principal in group.Members)
            {
                if (principal is UserPrincipal)
                {
                    if (principal.Sid == user.Sid)
                        return true;
                }
            }

            return false;
        }

        private bool IsUserInGroup(UserInformation userInfo, GroupInformation groupInfo)
        {
            if (groupInfo.SID != null)
            {
                using (UserPrincipal user = GetUserPrincipal(userInfo.Username))
                {
                    using (GroupPrincipal group = GetGroupPrincipal(groupInfo.SID))
                    {
                        return IsUserInGroup(user, group);
                    }
                }
            }
            else
            {
                using (UserPrincipal user = GetUserPrincipal(userInfo.Username))
                {
                    using (GroupPrincipal group = GetGroupPrincipal(groupInfo.Name))
                    {
                        return IsUserInGroup(user, group);
                    }
                }
            }             
        }

        private GroupPrincipal CreateOrGetGroupPrincipal(GroupInformation groupInfo)
        {
            GroupPrincipal group = null;

            // If we have a SID, use that, otherwise name
            if (groupInfo.SID != null)
                group = GetGroupPrincipal(groupInfo.SID);
            else
                group = GetGroupPrincipal(groupInfo.Name);
          
            if (group == null)
            {
                // We create the GroupPrincipal, but https://connect.microsoft.com/VisualStudio/feedback/details/525688/invalidoperationexception-with-groupprincipal-and-sam-principalcontext-for-setting-any-property-always
                // prevents us from then setting stuff on it.. so we then have to locate its relative DE 
                // and modify *that* instead.  Oi.
                using (group = new GroupPrincipal(m_machinePrincipal))
                {
                    group.Name = groupInfo.Name;
                    group.Save();

                    using (DirectoryEntry newGroupDe = m_sam.Children.Add(groupInfo.Name, "Group"))
                    {
                        if (!string.IsNullOrEmpty(groupInfo.Description))
                        {
                            newGroupDe.Properties["Description"].Value = groupInfo.Description;
                            newGroupDe.CommitChanges();
                        }                        
                    }

                    // We have to re-fetch to get changes made via underlying DE
                    return GetGroupPrincipal(group.Sid);
                }
            }
            
            return group;
        }

        private UserPrincipal CreateOrGetUserPrincipal(UserInformation userInfo)
        {
            UserPrincipal user = GetUserPrincipal(userInfo.Username);
            if (user == null)
            {
                // See note about MS bug in CreateOrGetGroupPrincipal to understand the mix of DE/Principal here:
                using (user = new UserPrincipal(m_machinePrincipal))
                {
                    user.Name = userInfo.Username;
                    user.Save();

                    // Sync via DE
                    SyncUserPrincipalInfo(user, userInfo);
                    
                    // We have to re-fetch to get changes made via underlying DE
                    return GetUserPrincipal(user.Name);
                }
            }

            return user;
        }

        private void SyncUserPrincipalInfo(UserPrincipal user, UserInformation info)
        {
            using(DirectoryEntry userDe = m_sam.Children.Find(info.Username, "User"))
            {
                if(!string.IsNullOrEmpty(info.Description)) userDe.Properties["Description"].Value = info.Description;
                if(!string.IsNullOrEmpty(info.Fullname)) userDe.Properties["FullName"].Value = info.Fullname;
                userDe.Invoke("SetPassword", new object[] { info.Password });
                userDe.CommitChanges();                
            }
        }

        private void AddUserToGroup(UserPrincipal user, GroupPrincipal group)
        {
            group.Members.Add(user);
            group.Save();
        }

        private void RemoveUserFromGroup(UserPrincipal user, GroupPrincipal group)
        {
            group.Members.Remove(user);
            group.Save();
        }

        public void SyncToLocalUser()
        {
            using (UserPrincipal user = CreateOrGetUserPrincipal(UserInfo))
            {
                // Force password and fullname match (redundant if we just created, but oh well)
                SyncUserPrincipalInfo(user, UserInfo);

                try
                {
                    List<SecurityIdentifier> ignoredSids = new List<SecurityIdentifier>(new SecurityIdentifier[] {
                        new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),    // "Authenticated Users"
                        new SecurityIdentifier("S-1-1-0"),                                      // "Everyone"                        
                    });
                    
                    // First remove from any local groups they aren't supposed to be in
                    foreach (GroupPrincipal group in user.GetAuthorizationGroups())
                    {
                        // Skip ignored sids
                        if (ignoredSids.Contains(group.Sid)) continue;

                        GroupInformation gi = new GroupInformation() { Name = group.Name, SID = group.Sid, Description = group.Description };
                        if (!UserInfo.InGroup(gi))
                        {
                            RemoveUserFromGroup(user, group);
                        }
                    }

                    // Now add to any they aren't already in that they should be
                    foreach (GroupInformation groupInfo in UserInfo.Groups)
                    {
                        if (!IsUserInGroup(UserInfo, groupInfo))
                        {
                            using (GroupPrincipal group = CreateOrGetGroupPrincipal(groupInfo))
                            {
                                AddUserToGroup(user, group);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new GroupSyncException(e);
                }
            }       
        }

        public static void SyncUserInfoToLocalUser(UserInformation userInfo)
        {
            LocalAccount la = new LocalAccount(userInfo);
            la.SyncToLocalUser();
        }
                                
        // Load userInfo.Username's group list and populate userInfo.Groups accordingly
        public static void SyncLocalGroupsToUserInfo(UserInformation userInfo)
        {
            ILog logger = LogManager.GetLogger("LocalAccount.SyncLocalGroupsToUserInfo");
            try
            {
                SecurityIdentifier EveryoneSid = new SecurityIdentifier("S-1-1-0");
                SecurityIdentifier AuthenticatedUsersSid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);

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
            catch(Exception e)
            {
                logger.ErrorFormat("Unexpected error while syncing local groups, skipping rest: {0}", e);
            }
        }
    }
}
