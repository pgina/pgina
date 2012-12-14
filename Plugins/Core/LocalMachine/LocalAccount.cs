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
using System.Security.AccessControl;
using System.IO;

using pGina.Shared.Types;

namespace pGina.Plugin.LocalMachine
{
    public class LocalAccount
    {
        private static ILog m_logger = null; 
        private static DirectoryEntry m_sam = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
        private static PrincipalContext m_machinePrincipal = new PrincipalContext(ContextType.Machine);
        
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
        
        static LocalAccount()
        {
            m_logger = LogManager.GetLogger("LocalAccount");
        }

        public LocalAccount(UserInformation userInfo)
        {
            UserInfo = userInfo;            
        }        

        /// <summary>
        /// Finds and returns the UserPrincipal object if it exists, if not, returns null.
        /// This method uses PrincipalSearcher because it is faster than UserPrincipal.FindByIdentity.
        /// The username comparison is case insensitive.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>The UserPrincipal object, or null if not found.</returns>
        public static UserPrincipal GetUserPrincipal(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;

            // Since PrincipalSearcher is case sensitive, and we want a case insensitive 
            // search, we get a list of all users and compare the names "manually."
            using (PrincipalSearcher searcher = new PrincipalSearcher(new UserPrincipal(m_machinePrincipal)))
            {
                PrincipalSearchResult<Principal> sr = searcher.FindAll();
                foreach (Principal p in sr)
                {
                    if (p is UserPrincipal)
                    {
                        UserPrincipal user = (UserPrincipal)p;
                        if (user.Name.Equals(username, StringComparison.CurrentCultureIgnoreCase))
                            return user;
                    }
                }
            }

            return null;
        }

        public static UserPrincipal GetUserPrincipal(SecurityIdentifier sid)
        {
            // This could be updated to use PrincipalSearcher, but the method is currently
            // unused.
            return UserPrincipal.FindByIdentity(m_machinePrincipal, IdentityType.Sid, sid.ToString());
        }

        /// <summary>
        /// Finds and returns the GroupPrincipal object if it exists, if not, returns null.
        /// This method uses PrincipalSearcher because it is faster than GroupPrincipal.FindByIdentity.
        /// The group name comparison is case insensitive.
        /// </summary>
        /// <param name="groupname"></param>
        /// <returns></returns>
        public static GroupPrincipal GetGroupPrincipal(string groupname)
        {
            if (string.IsNullOrEmpty(groupname)) return null;

            // In order to do a case insensitive search, we need to scan all
            // groups "manually."
            using(PrincipalSearcher searcher = new PrincipalSearcher(new GroupPrincipal(m_machinePrincipal))) 
            {
                PrincipalSearchResult<Principal> sr = searcher.FindAll();
                foreach (Principal p in sr)
                {
                    if (p is GroupPrincipal)
                    {
                        GroupPrincipal group = (GroupPrincipal)p;
                        if (group.Name.Equals(groupname, StringComparison.CurrentCultureIgnoreCase))
                            return group;
                    }
                }
            }
            return null;
        }

        public static GroupPrincipal GetGroupPrincipal(SecurityIdentifier sid)
        {
            using (PrincipalSearcher searcher = new PrincipalSearcher(new GroupPrincipal(m_machinePrincipal)))
            {
                PrincipalSearchResult<Principal> sr = searcher.FindAll();
                foreach (Principal p in sr)
                {
                    if (p is GroupPrincipal)
                    {
                        GroupPrincipal group = (GroupPrincipal)p;
                        if (group.Sid == sid)
                            return group;
                    }
                }
            }
            return null;
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
        private static bool IsUserInGroup(UserPrincipal user, GroupPrincipal group)
        {
            if (user == null || group == null) return false;

            // This may seem a convoluted and strange way to check group membership.  
            // Especially because I could just call user.IsMemberOf(group).  
            // The reason for all of this is that IsMemberOf will throw an exception
            // if there is an unresolvable SID in the list of group members.  Unfortunately,
            // even looping over the members with a standard foreach loop doesn't allow
            // for catching the exception and continuing.  Therefore, we need to use the
            // IEnumerator object and iterate through the members carefully, catching the
            // exception if it is thrown.  I throw in a sanity check because there's no
            // guarantee that MoveNext will actually move the enumerator forward when an
            // exception occurs, although it has done so in my tests.
            //
            // For additional details, see the following bug:
            // https://connect.microsoft.com/VisualStudio/feedback/details/453812/principaloperationexception-when-enumerating-the-collection-groupprincipal-members

            PrincipalCollection members = group.Members;
            bool ok = true;
            int errorCount = 0;  // This is a sanity check in case the loop gets out of control
            IEnumerator<Principal> membersEnum = members.GetEnumerator();
            while (ok)
            {
                try { ok = membersEnum.MoveNext(); }
                catch (PrincipalOperationException)
                {
                    m_logger.ErrorFormat("PrincipalOperationException when checking group membership for user {0} in group {1}." +
                        "  This usually means that you have an unresolvable SID as a group member." +
                        "  I strongly recommend that you fix this problem as soon as possible by removing the SID from the group. " +
                        "  Ignoring the exception and continuing.",
                        user.Name, group.Name);

                    // Sanity check to avoid infinite loops
                    errorCount++;
                    if (errorCount > 1000) return false;  
                    
                    continue;
                }
                
                if (ok)
                {
                    Principal principal = membersEnum.Current;
                    
                    if (principal is UserPrincipal && principal.Sid == user.Sid)
                        return true;
                }
            }

            return false;
        }

        private bool IsUserInGroup(UserPrincipal user, GroupInformation groupInfo)
        {
            using (GroupPrincipal group = GetGroupPrincipal(groupInfo.Name))
            {
                return IsUserInGroup(user, group);
            }
        }

        private GroupPrincipal CreateOrGetGroupPrincipal(GroupInformation groupInfo)
        {
            GroupPrincipal group = null;

            // If we have a SID, use that, otherwise name
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
                    return GetGroupPrincipal(group.Name);
                }
            }
            
            return group;
        }

        private UserPrincipal CreateOrGetUserPrincipal(UserInformation userInfo)
        {
            UserPrincipal user = null;
            if ( ! LocalAccount.UserExists(userInfo.Username) )
            {
                // See note about MS bug in CreateOrGetGroupPrincipal to understand the mix of DE/Principal here:
                using (user = new UserPrincipal(m_machinePrincipal))
                {
                    user.Name = userInfo.Username;
                    user.SetPassword(userInfo.Password);
                    user.Save();

                    // Sync via DE
                    SyncUserPrincipalInfo(user, userInfo);
                    
                    // We have to re-fetch to get changes made via underlying DE
                    return GetUserPrincipal(user.Name);
                }
            }

            user = GetUserPrincipal(userInfo.Username);
            if (user != null)
                return user;
            else
                throw new Exception(
                    String.Format("Unable to get user principal for account that apparently exists: {0}", userInfo.Username));
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
            m_logger.Debug("SyncToLocalUser()");
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
                    m_logger.Debug("Checking for groups to remove.");
                    List<GroupPrincipal> localGroups = LocalAccount.GetGroups(user);
                    foreach (GroupPrincipal group in localGroups)
                    {
                        m_logger.DebugFormat("Remove {0}?", group.Name);
                        // Skip ignored sids
                        if (!ignoredSids.Contains(group.Sid))
                        {
                            GroupInformation gi = new GroupInformation() { Name = group.Name, SID = group.Sid, Description = group.Description };
                            if (!UserInfo.InGroup(gi))
                            {
                                m_logger.DebugFormat("Removing user {0} from group {1}", user.Name, group.Name);
                                RemoveUserFromGroup(user, group);
                            }
                        }
                        group.Dispose();
                    }

                    // Now add to any they aren't already in that they should be
                    m_logger.Debug("Checking for groups to add");
                    foreach (GroupInformation groupInfo in UserInfo.Groups)
                    {
                        m_logger.DebugFormat("Add {0}?", groupInfo.Name);
                        if (!IsUserInGroup(user, groupInfo))
                        {
                            using (GroupPrincipal group = CreateOrGetGroupPrincipal(groupInfo))
                            {
                                m_logger.DebugFormat("Adding user {0} to group {1}", user.Name, group.Name);
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
            m_logger.Debug("End SyncToLocalUser()");
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

                if (LocalAccount.UserExists(userInfo.Username))
                {
                    using (UserPrincipal user = LocalAccount.GetUserPrincipal(userInfo.Username))
                    {
                        foreach (GroupPrincipal group in LocalAccount.GetGroups(user))
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
            catch(Exception e)
            {
                logger.ErrorFormat("Unexpected error while syncing local groups, skipping rest: {0}", e);
            }
        }

        public static void RemoveUserAndProfile(string user)
        {
            using (UserPrincipal userPrincipal = GetUserPrincipal(user))
            {
                // First we have to work out where the users profile is on disk.
                try
                {
                    string usersProfileDir = Abstractions.Windows.User.GetProfileDir(userPrincipal.Sid);
                    if (!string.IsNullOrEmpty(usersProfileDir))
                    {
                        m_logger.DebugFormat("User {0} has profile in {1}, giving myself delete permission", user, usersProfileDir);
                        RecurseDelete(usersProfileDir);
                        // Now remove it from the registry as well
                        Abstractions.WindowsApi.pInvokes.DeleteProfile(userPrincipal.Sid);
                    }
                }
                catch (KeyNotFoundException) 
                {
                    m_logger.DebugFormat("User {0} has no disk profile, just removing principal", user);
                }
                userPrincipal.Delete();                
            }
        }

        private static void RecurseDelete(string directory)
        {
            // m_logger.DebugFormat("Dir: {0}", directory);
            DirectorySecurity dirSecurity = Directory.GetAccessControl(directory);
            dirSecurity.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, FileSystemRights.FullControl, AccessControlType.Allow));
            Directory.SetAccessControl(directory, dirSecurity);
            File.SetAttributes(directory, FileAttributes.Normal);

            DirectoryInfo di = new DirectoryInfo(directory);
            if ((di.Attributes & FileAttributes.ReparsePoint) != 0)
            {
                // m_logger.DebugFormat("{0} is a reparse point, just deleting without recursing", directory);
                Directory.Delete(directory, false);
                return;
            }

            string[] files = Directory.GetFiles(directory);
            string[] dirs = Directory.GetDirectories(directory);

            // Files
            foreach (string file in files)
            {
                // m_logger.DebugFormat("File: {0}", file);
                FileSecurity fileSecurity = File.GetAccessControl(file);
                fileSecurity.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(file, fileSecurity); // Set the new access settings.
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            // Recurse each dir
            foreach (string dir in dirs)
            {
                RecurseDelete(dir);
            }
            
            Directory.Delete(directory, false);
        }

        /// <summary>
        /// This is a faster technique for determining whether or not a user exists on the local
        /// machine.  UserPrincipal.FindByIdentity tends to be quite slow in general, so if
        /// you only need to know whether or not the account exists, this method is much 
        /// faster.
        /// </summary>
        /// <param name="strUserName">The user name</param>
        /// <returns>Whether or not the account with the given user name exists on the system</returns>
        public static bool UserExists(string strUserName)
        {
            try
            {
                using (DirectoryEntry userEntry = LocalAccount.GetUserDirectoryEntry(strUserName))
                {
                    return userEntry != null;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a list of groups of which the user is a member.  It does so in a fashion that
        /// may seem strange since one can call UserPrincipal.GetGroups, but seems to be much faster
        /// in my tests.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static List<GroupPrincipal> GetGroups(UserPrincipal user)
        {
            List<GroupPrincipal> result = new List<GroupPrincipal>();

            // Get all groups using a PrincipalSearcher and 
            GroupPrincipal filter = new GroupPrincipal(m_machinePrincipal);
            using (PrincipalSearcher searcher = new PrincipalSearcher(filter))
            {
                PrincipalSearchResult<Principal> sResult = searcher.FindAll();
                foreach (Principal p in sResult)
                {
                    if (p is GroupPrincipal)
                    {
                        GroupPrincipal gp = (GroupPrincipal)p;
                        if (LocalAccount.IsUserInGroup(user, gp))
                            result.Add(gp);
                        else
                            gp.Dispose();
                    }
                    else
                    {
                        p.Dispose();
                    }
                }
            }
            return result;
        }
        
    }
}
