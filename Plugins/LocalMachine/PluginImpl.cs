/*
	Copyright (c) 2014, pGina Team
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
using System.IO;

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using Abstractions;

namespace pGina.Plugin.LocalMachine
{

    public class PluginImpl : IPluginAuthentication, IPluginAuthorization, IPluginAuthenticationGateway, IPluginConfiguration, IPluginEventNotifications, IPluginLogoffRequestAddTime, IPluginChangePassword
    {
        // Per-instance logger
        private ILog m_logger = LogManager.GetLogger("LocalMachine");

        private Dictionary<string, Boolean> RunningTasks = new Dictionary<string, Boolean>();
        private ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        public static Boolean IsShuttingDown = false;

        #region Init-plugin
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
        #endregion

        public void Starting() { }
        public void Stopping() { }

        public Boolean LogoffRequestAddTime()
        {
            IsShuttingDown = true;
            try
            {
                Locker.TryEnterReadLock(-1);
                if (RunningTasks.Values.Contains(true))
                    return true;
            }
            catch (Exception ex)
            {
                m_logger.InfoFormat("LogoffRequestAddTime() error {0}", ex.Message);
            }
            finally
            {
                Locker.ExitReadLock();
            }

            return false;
        }

        public Boolean LoginUserRequest(string username)
        {
            try
            {
                Locker.TryEnterReadLock(-1);
                if (RunningTasks.Keys.Contains(username.ToLower()))
                {
                    m_logger.InfoFormat("LoginUserRequest() logoff in process for {0}", username);
                    return true;
                }
                else
                {
                    m_logger.InfoFormat("LoginUserRequest() {0} free to login", username);
                    return false;
                }
            }
            catch (Exception ex)
            {
                m_logger.InfoFormat("LoginUserRequest() {0} error {1}", username, ex.Message);
            }
            finally
            {
                Locker.ExitReadLock();
            }

            return false;
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

        public BooleanResult ChangePassword(SessionProperties properties, ChangePasswordPluginActivityInfo pluginInfo)
        {
            m_logger.Debug("ChangePassword()");

            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            // Verify the old password
            if (Abstractions.WindowsApi.pInvokes.ValidateCredentials(userInfo.Username, userInfo.oldPassword))
            {
                m_logger.DebugFormat("Authenticated via old password: {0}", userInfo.Username);
            }
            else
            {
                return new BooleanResult { Success = false, Message = "Current password or username is not valid." };
            }

            using (UserPrincipal user = LocalAccount.GetUserPrincipal(userInfo.Username))
            {
                if (user != null)
                {
                    m_logger.DebugFormat("Found principal, changing password for {0}", userInfo.Username);
                    user.SetPassword(userInfo.Password);
                }
                else
                {
                    return new BooleanResult { Success = false, Message = "Local machine plugin internal error: directory entry not found." };
                }
            }

            return new BooleanResult { Success = true, Message = "Local password successfully changed." };
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

            // is this a pgina user?
            if (LocalAccount.UserExists(userInfo.Username) && !userInfo.Description.Contains("pGina created"))
            {
                m_logger.InfoFormat("User {0} is'nt a pGina created user. I'm not executing Gateway stage", userInfo.Username);
                return new BooleanResult() { Success = true };
            }

            // Add user to all mandatory groups
            if (MandatoryGroups.Length > 0)
            {
                foreach (string group in MandatoryGroups)
                {
                    string group_string=group;

                    m_logger.DebugFormat("Is there a Group with SID/Name:{0}", group);
                    using (GroupPrincipal groupconf = LocalAccount.GetGroupPrincipal(group))
                    {
                        if (groupconf != null)
                        {
                            m_logger.DebugFormat("Groupname: \"{0}\"", groupconf.Name);
                            group_string = groupconf.Name;
                        }
                        else
                        {
                            m_logger.ErrorFormat("Group: \"{0}\" not found", group);
                            m_logger.Error("Failsave add user to group Users");
                            using (GroupPrincipal groupfail = LocalAccount.GetGroupPrincipal(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null).ToString()))
                            {
                                if (groupfail != null)
                                {
                                    group_string = groupfail.Name;
                                }
                                else
                                {
                                    m_logger.Debug("no BuiltinUsers. I'm out of options");
                                    group_string = null;
                                }
                            }
                        }
                    }

                    if (group_string != null)
                        userInfo.AddGroup(new GroupInformation() { Name = group_string });
                }
            }

            try
            {
                m_logger.DebugFormat("AuthenticatedUserGateway({0}) for user: {1}", properties.Id.ToString(), userInfo.Username);
                LocalAccount.SyncUserInfoToLocalUser(userInfo);
                using (UserPrincipal user = LocalAccount.GetUserPrincipal(userInfo.Username))
                {
                    userInfo.SID = user.Sid;
                    userInfo.Description = user.Description;
                }
                properties.AddTrackedSingle<UserInformation>(userInfo);
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

        public void SessionChange(System.ServiceProcess.SessionChangeDescription changeDescription, SessionProperties properties)
        {
            if (properties == null || changeDescription == null)
                return;

            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            if (!userInfo.HasSID)
            {
                m_logger.InfoFormat("SessionChange Event denied for ID:{0}", changeDescription.SessionId);
                return;
            }

            if (changeDescription.Reason == System.ServiceProcess.SessionChangeReason.SessionLogoff)
            {
                m_logger.DebugFormat("SessionChange SessionLogoff for ID:{0} as user:{1}", changeDescription.SessionId, userInfo.Username);

                if (userInfo.Description.Contains("pGina created"))
                {
                    try
                    {
                        Locker.TryEnterWriteLock(-1);
                        RunningTasks.Add(userInfo.Username.ToLower(), true);
                    }
                    finally
                    {
                        Locker.ExitWriteLock();
                    }

                    Thread rem_local = new Thread(() => cleanup(userInfo, changeDescription.SessionId));
                    rem_local.Start();
                }
                else
                {
                    m_logger.InfoFormat("User {0} is'nt a pGina created user. I'm not executing Notification stage", userInfo.Username);
                }
            }
            if (changeDescription.Reason == System.ServiceProcess.SessionChangeReason.SessionLogon)
            {
                m_logger.DebugFormat("SessionChange SessionLogon for ID:{0} as user:{1}", changeDescription.SessionId, userInfo.Username);

                if (userInfo.Description.Contains("pGina created") && !userInfo.Description.Contains("pgSMB"))
                {
                    if (!String.IsNullOrEmpty(userInfo.LoginScript))
                    {
                        try
                        {
                            Abstractions.WindowsApi.pInvokes.StartUserProcessInSession(changeDescription.SessionId, userInfo.LoginScript);
                        }
                        catch (Exception ex)
                        {
                            m_logger.ErrorFormat("Can't run application {0} because {1}", userInfo.LoginScript, ex.ToString());
                        }
                    }

                    if (!Abstractions.Windows.User.QueryQuota(Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation.HKEY_USERS, userInfo.SID.ToString()) && Convert.ToUInt32(userInfo.usri4_max_storage) > 0)
                    {
                        m_logger.InfoFormat("no quota GPO settings for user {0}", userInfo.SID.ToString());
                        if (!Abstractions.Windows.User.SetQuota(Abstractions.WindowsApi.pInvokes.structenums.RegistryLocation.HKEY_USERS, userInfo.SID.ToString(), Convert.ToUInt32(userInfo.usri4_max_storage)))
                        {
                            m_logger.InfoFormat("failed to set quota GPO for user {0}", userInfo.SID.ToString());
                        }
                        else
                        {
                            m_logger.InfoFormat("done quota GPO settings for user {0}", userInfo.SID.ToString());
                            try
                            {
                                Abstractions.WindowsApi.pInvokes.StartUserProcessInSession(changeDescription.SessionId, "proquota.exe");
                            }
                            catch (Exception ex)
                            {
                                m_logger.ErrorFormat("Can't run application {0} because {1}", "proquota.exe", ex.ToString());
                            }
                        }
                    }
                }
                else
                {
                    m_logger.InfoFormat("User {0} is'nt a pGina created user. I'm not executing Notification stage", userInfo.Username);
                }
            }
        }

        private void cleanup(UserInformation userInfo, int sessionID)
        {
            bool scramble = Settings.Store.ScramblePasswords;
            bool remove = Settings.Store.RemoveProfiles;

            while (true)
            {
                // logoff detection is quite a problem under NT6
                // a disconnectEvent is only triggered during a logoff
                // but not during a shutdown/reboot
                // and the SessionLogoffEvent is only saying that the user is logging of
                // So, there is no event that is fired during a user-logoff/reboot/shutdown
                // that indicates that the user has logged of
                if (Abstractions.WindowsApi.pInvokes.IsSessionLoggedOFF(sessionID) || IsShuttingDown)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }

            m_logger.DebugFormat("start cleanup for user {0} with Description \"{1}\"", userInfo.Username, userInfo.Description);

            if (LocalAccount.UserExists(userInfo.Username))
            {
                if (remove)
                {
                    m_logger.DebugFormat("remove profile {0}", userInfo.Username);
                    LocalAccount.RemoveUserAndProfile(userInfo.Username, sessionID);
                }
                else
                {
                    m_logger.DebugFormat("not removing profile {0}", userInfo.Username);
                }
                if (scramble && !remove)
                {
                    m_logger.DebugFormat("scramble password {0}", userInfo.Username);
                    LocalAccount.ScrambleUsersPassword(userInfo.Username);
                }
                else
                {
                    m_logger.DebugFormat("not scramble password {0}", userInfo.Username);
                }
                m_logger.DebugFormat("cleanup done for user {0}", userInfo.Username);
            }
            else
            {
                m_logger.Debug(userInfo.Username + " doesnt exist");
            }

            try
            {
                Locker.TryEnterWriteLock(-1);
                RunningTasks.Remove(userInfo.Username.ToLower());
            }
            finally
            {
                Locker.ExitWriteLock();
            }
        }
    }
}
