/*
	Copyright (c) 2012, pGina Team
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
using System.Security.Principal;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading;

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;

namespace pGina.Plugin.LocalMachine
{

    public class PluginImpl : IPluginAuthentication, IPluginAuthorization, IPluginAuthenticationGateway, IPluginConfiguration, IPluginChangePassword
    {
        // Per-instance logger
        private ILog m_logger = LogManager.GetLogger("LocalMachine");

        private Timer m_backgroundTimer = null;

        public static Guid PluginUuid
        {
            get { return new Guid("{12FA152D-A2E3-4C8D-9535-5DCD49DFCB6D}"); }
        }
        
        public PluginImpl()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }
 
        public string Name
        {
            get { return "Local Machine"; }
        }

        public string Description
        {
            get { return "Manages local machine accounts for authenticated users, and authenticates against the local SAM"; }
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

        private int FindString(string[] array, string filter)
        {
            for (int x = 0; x < array.Length; x++)
            {
                if (array[x].StartsWith(filter))
                    return x;
            }

            return -1;
        }
        
        public BooleanResult AuthenticatedUserGateway(SessionProperties properties)
        {
            // Our job, if we've been elected to do gateway, is to ensure that an
            //  authenticated user:
            //
            //  1. Has a local account
            //  2. That account's password is set to the one they used to authenticate
            //  3. That account is a member of all groups listed, and not a member of any others
            
            // Is failure at #3 a total fail?
            bool failIfGroupSyncFails = Settings.Store.GroupCreateFailIsFail;

            // Groups everyone is added to
            string[] MandatoryGroups = Settings.Store.MandatoryGroups;

            // user info
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            // Add user to all mandatory groups
            if (MandatoryGroups.Length > 0)
            {
                foreach (string group in MandatoryGroups)
                    userInfo.AddGroup(new GroupInformation() { Name = group });
            }

            try
            {
                bool scramble = Settings.Store.ScramblePasswords;
                bool remove = Settings.Store.RemoveProfiles;

                if (remove)
                {
                    // If this user doesn't already exist, and we are supposed to clean up after ourselves,
                    //  make note of the username!
                    if (!LocalAccount.UserExists(userInfo.Username))
                    {
                        m_logger.DebugFormat("Marking for deletion: {0}", userInfo.Username);
                        CleanupTasks.AddTask(new CleanupTask(userInfo.Username, CleanupAction.DELETE_PROFILE));
                    }
                }

                // If we are configured to scramble passwords
                if (scramble)
                {
                    // Scramble the password only if the user is not in the list
                    // of exceptions.
                    string[] exceptions = Settings.Store.ScramblePasswordsExceptions;
                    if (!exceptions.Contains(userInfo.Username, StringComparer.CurrentCultureIgnoreCase))
                    {
                        // If configured to do so, we check to see if this plugin failed
                        // to auth this user, and only scramble in that case
                        bool scrambleWhenLMFail = Settings.Store.ScramblePasswordsWhenLMAuthFails;
                        if (scrambleWhenLMFail)
                        {
                            // Scramble the password only if we did not authenticate this user
                            if (!DidWeAuthThisUser(properties, false))
                            {
                                m_logger.DebugFormat("LM did not authenticate this user, marking user for scramble: {0}", userInfo.Username);
                                CleanupTasks.AddTask(new CleanupTask(userInfo.Username, CleanupAction.SCRAMBLE_PASSWORD));
                            }
                        }
                        else
                        {
                            m_logger.DebugFormat("Marking user for scramble: {0}", userInfo.Username);
                            CleanupTasks.AddTask(new CleanupTask(userInfo.Username, CleanupAction.SCRAMBLE_PASSWORD));
                        }
                    }
                }
                
                m_logger.DebugFormat("AuthenticatedUserGateway({0}) for user: {1}", properties.Id.ToString(), userInfo.Username);
                LocalAccount.SyncUserInfoToLocalUser(userInfo);
            }
            catch (LocalAccount.GroupSyncException e)
            {
                if (failIfGroupSyncFails)
                    return new BooleanResult() { Success = false, Message = string.Format("Unable to sync users local group membership: {0}", e.RootException) };
            }
            catch(Exception e)
            {
                return new BooleanResult() { Success = false, Message = string.Format("Unexpected error while syncing user's info: {0}", e) };
            }

            return new BooleanResult() { Success = true };
        }

        private bool HasUserAuthenticatedYet(SessionProperties properties)
        {
            PluginActivityInformation pluginInfo = properties.GetTrackedSingle<PluginActivityInformation>();
            foreach (Guid uuid in pluginInfo.GetAuthenticationPlugins())
            {
                if (pluginInfo.GetAuthenticationResult(uuid).Success)
                    return true;
            }

            return false;
        }

        public BooleanResult AuthenticateUser(SessionProperties properties)
        {
            try
            {
                bool alwaysAuth = Settings.Store.AlwaysAuthenticate; 
                
                m_logger.DebugFormat("AuthenticateUser({0})", properties.Id.ToString());

                // Get user info
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();                
                m_logger.DebugFormat("Found username: {0}", userInfo.Username);                               

                // Should we authenticate? Only if user has not yet authenticated, or we are not in fallback mode                
                if (alwaysAuth || !HasUserAuthenticatedYet(properties))
                {
                    if (LocalAccount.UserExists(userInfo.Username))
                    {
                        // We use a pInvoke here instead of using PrincipalContext.ValidateCredentials
                        // due to the fact that the latter will throw an exception when the network is disconnected.
                        if (Abstractions.WindowsApi.pInvokes.ValidateCredentials(userInfo.Username, userInfo.Password))
                        {
                            m_logger.InfoFormat("Authenticated user: {0}", userInfo.Username);
                            userInfo.Domain = Environment.MachineName;

                            m_logger.Debug("AuthenticateUser: Mirroring group membership from SAM");
                            LocalAccount.SyncLocalGroupsToUserInfo(userInfo);

                            // Return success
                            return new BooleanResult() { Success = true };
                        }
                    }
                    else
                    {
                        m_logger.InfoFormat("User {0} does not exist on this machine.", userInfo.Username);
                    }
                }        

                m_logger.ErrorFormat("Failed to authenticate user: {0}", userInfo.Username);
                // Note that we don't include a message.  We are a last chance auth, and want previous/failed plugins
                //  to have the honor of explaining why.
                return new BooleanResult() { Success = false, Message = null }; 
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("AuthenticateUser exception: {0}", e);
                throw;  // Allow pGina service to catch and handle exception
            }
        }

        public BooleanResult AuthorizeUser(SessionProperties properties)
        {
            // Some things we always do,
            bool mirrorGroups = Settings.Store.MirrorGroupsForAuthdUsers;   // Should we load users groups from SAM? We always do if we auth'd only
            if (mirrorGroups && !DidWeAuthThisUser(properties, false))
            {
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
                if (LocalAccount.UserExists(userInfo.Username))
                {
                    m_logger.DebugFormat("AuthorizeUser: Mirroring users group membership from SAM");
                    LocalAccount.SyncLocalGroupsToUserInfo(userInfo);
                }
            }

            // Do we need to do authorization?
            if (DoesAuthzApply(properties))
            {
                bool limitToLocalAdmins = Settings.Store.AuthzLocalAdminsOnly;
                bool limitToLocalGroups = Settings.Store.AuthzLocalGroupsOnly;
                string[] limitToGroupList = Settings.Store.AuthzLocalGroups;
                bool restrictionsApply = limitToLocalAdmins || limitToLocalGroups;
                PluginActivityInformation pluginInfo = properties.GetTrackedSingle<PluginActivityInformation>();

                if (!restrictionsApply)
                {
                    return new BooleanResult() { Success = true };
                }
                else if (!pluginInfo.LoadedAuthenticationGatewayPlugins.Contains(this))
                {
                    return new BooleanResult()
                    {
                        Success = false,
                        Message = string.Format("Plugin configured to authorize users based on group membership, but not in the gateway list to ensure membership is enforced, denying access")
                    };
                }

                // The user must have the local administrator group in his group list, and 
                //  we must be in the Gateway list of plugins (as we'll be the ones ensuring
                //  this group membership is enforced).
                if (limitToLocalAdmins)
                {
                    SecurityIdentifier adminSid = Abstractions.Windows.Security.GetWellknownSID(WellKnownSidType.BuiltinAdministratorsSid);
                    string adminName = Abstractions.Windows.Security.GetNameFromSID(adminSid);                    
                   
                    if(!ListedInGroup(adminName, adminSid, properties))
                    {
                        return new BooleanResult()
                        {
                            Success = false,
                            Message = string.Format("Users group list does not include the admin group ({0}), denying access", adminName)
                        };
                    }
                }

                // The user must have one of the groups listed (by name) in their group list
                // and we must be in the Gateway list of plugins (as we'll be the ones ensuring
                //  this group membership is enforced).
                if (limitToLocalGroups)
                {
                    if (limitToGroupList.Length > 0)
                    {
                        foreach (string group in limitToGroupList)
                        {
                            if (ListedInGroup(group, null, properties))
                            {
                                return new BooleanResult() { Success = true };
                            }
                        }
                    }
                    return new BooleanResult()
                    {
                        Success = false,
                        Message = "User is not a member of one of the required groups, denying access"
                    };
                }

                return new BooleanResult() { Success = true };
            }
            else
            {
                // We elect to not do any authorization, let the user pass for us
                return new BooleanResult() { Success = true };
            }
        }        

        public void Configure()
        {
            Configuration dialog = new Configuration();
            dialog.ShowDialog();
        }

        private bool ListedInGroup(string name, SecurityIdentifier sid, SessionProperties properties)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            foreach (GroupInformation group in userInfo.Groups)
            {
                if (group.Name == name || (sid != null && group.SID == sid))
                    return true;
            }

            return false;
        }

        private bool DoesAuthzApply(SessionProperties properties)
        {
            // Do we authorize all users?
            bool authzAllUsers = Settings.Store.AuthzApplyToAllUsers;
            if (authzAllUsers) return true;
            
            // Did we auth this user?
            return DidWeAuthThisUser(properties, false);
        }

        private bool DidWeAuthThisUser(SessionProperties properties, bool exclusiveOnly)
        {
            PluginActivityInformation pluginInfo = properties.GetTrackedSingle<PluginActivityInformation>();

            if (!exclusiveOnly)
            {
                if (pluginInfo.GetAuthenticationPlugins().Contains(PluginUuid))
                {
                    return pluginInfo.GetAuthenticationResult(PluginUuid).Success;
                }
            }
            else
            {
                if (!pluginInfo.GetAuthenticationPlugins().Contains(PluginUuid))
                    return false;

                // We must be the only one
                foreach (Guid pluginId in pluginInfo.GetAuthenticationPlugins())
                {
                    if (pluginId != PluginUuid && pluginInfo.GetAuthenticationResult(pluginId).Success) return false;
                }

                return true;
            }

            return false;
        }
        
        public void Starting()
        {         
            // Start background timer          
            m_logger.DebugFormat("Starting background timer");  
            m_backgroundTimer = new Timer(new TimerCallback(BackgroundTaskCallback), null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(-1));            
        }

        public void Stopping()
        {            
            // Dispose of our background timer            
            lock (this)
            {
                m_logger.DebugFormat("Stopping background timer");
                m_backgroundTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
                m_backgroundTimer.Dispose();
                m_backgroundTimer = null;
            }
        }

        private TimeSpan BackgroundTimeSpan
        {
            get
            {
                int seconds = Settings.Store.BackgroundTimerSeconds;
                return TimeSpan.FromSeconds(seconds);
            }
        }

        private void BackgroundTaskCallback(object state)
        {
            // Do background stuff
            lock(this)
            {
                try
                {
                    IterateCleanupUsers();
                }
                catch (Exception e)
                {
                    // Log the exception and continue
                    m_logger.ErrorFormat("Exception in IterateCleanupUsers {0}", e);
                }

                if (m_backgroundTimer != null)
                    m_backgroundTimer.Change(BackgroundTimeSpan, TimeSpan.FromMilliseconds(-1));
            }
        }

        private List<string> LoggedOnLocalUsers()
        {
            List<string> loggedOnUsers = Abstractions.WindowsApi.pInvokes.GetInteractiveUserList();

            // Transform the logged on list, strip it to just local users, then drop the machine name
            List<string> xformed = new List<string>();
            foreach (string user in loggedOnUsers)
            {
                if (user.Contains("\\"))
                {
                    if (user.StartsWith(Environment.MachineName,StringComparison.CurrentCultureIgnoreCase))
                        xformed.Add(user.Substring(user.IndexOf("\\") + 1).ToUpper());
                }
                else
                    xformed.Add(user.ToUpper());
            }
            return xformed;
        }

        private void IterateCleanupUsers()
        {
            lock(this)
            {
                List<CleanupTask> tasks = CleanupTasks.GetEligibleTasks();
                List<string> loggedOnUsers = null;
                try
                {
                    loggedOnUsers = LoggedOnLocalUsers();
                }
                catch (System.ComponentModel.Win32Exception e)
                {
                    m_logger.ErrorFormat("Error (ignored) LoggedOnLocalUsers {0}", e);
                    return;
                }

                m_logger.DebugFormat("IterateCleanupUsers Eligible users: {0}", string.Join(",",tasks));
                m_logger.DebugFormat("IterateCleanupUsers loggedOnUsers: {0}", string.Join(",",loggedOnUsers));

                foreach (CleanupTask task in tasks)
                {
                    try
                    {
                        using (UserPrincipal userPrincipal = LocalAccount.GetUserPrincipal(task.UserName))
                        {
                            // Make sure the user exists
                            if (userPrincipal == null)
                            {
                                // This dude doesn't exist!
                                m_logger.DebugFormat("User {0} doesn't exist, not cleaning up.", task.UserName);
                                CleanupTasks.RemoveTaskForUser(task.UserName);
                                continue;
                            }

                            // Is she logged in still?
                            if (loggedOnUsers.Contains(task.UserName, StringComparer.CurrentCultureIgnoreCase))
                                continue;

                            m_logger.InfoFormat("Cleaning up: {0} -> {1}", task.UserName, task.Action);

                            try
                            {
                                switch (task.Action)
                                {
                                    case CleanupAction.SCRAMBLE_PASSWORD:
                                        LocalAccount.ScrambleUsersPassword(task.UserName);
                                        break;
                                    case CleanupAction.DELETE_PROFILE:
                                        LocalAccount.RemoveUserAndProfile(task.UserName);
                                        break;
                                    default:
                                        m_logger.ErrorFormat("Unrecognized action: {0}, skipping user {1}", task.Action, task.UserName);
                                        throw new Exception();
                                }
                            }
                            catch (Exception e)
                            {
                                m_logger.WarnFormat("Cleanup for {0} failed, will retry next time around. Error: {1}", task.UserName, e);
                                continue;
                            }

                            // All done! No more cleanup for this user needed
                            CleanupTasks.RemoveTaskForUser(task.UserName);
                        }
                    }
                    catch (Exception e)
                    {
                        // If something goes wrong, we log the exception and ignore.
                        m_logger.ErrorFormat("Caught (ignoring) Exception {0}", e);
                    }
                }
            }
        }

        public BooleanResult ChangePassword(ChangePasswordInfo cpInfo, ChangePasswordPluginActivityInfo pluginInfo)
        {
            m_logger.Debug("ChangePassword()");

            // Verify the old password
            if (Abstractions.WindowsApi.pInvokes.ValidateCredentials(cpInfo.Username, cpInfo.OldPassword))
            {
                m_logger.DebugFormat("Authenticated via old password: {0}", cpInfo.Username);
            }
            else
            {
                return new BooleanResult { Success = false, Message = "Current password or username is not valid." };
            }

            using (UserPrincipal user = LocalAccount.GetUserPrincipal(cpInfo.Username))
            {
                if (user != null)
                {
                    m_logger.DebugFormat("Found principal, changing password for {0}", cpInfo.Username);
                    user.SetPassword(cpInfo.NewPassword);
                }
                else
                {
                    return new BooleanResult { Success = false, Message = "Local machine plugin internal error: directory entry not found." };
                }
            }

            return new BooleanResult { Success = true, Message = "Local password successfully changed." };
        }
    }
}
