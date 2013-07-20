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
using System.ServiceProcess;
using System.Collections.Generic;

namespace pGina.Shared.Interfaces
{    
    /// <summary>
    /// All plugins must implement this interface 
    /// </summary>
    public interface IPluginBase
    {
        string Name { get; }
        string Description { get; }
        string Version { get; }
        Guid Uuid { get; }

        /// <summary>
        /// Called when pGina service starts.  Intended for 'startup' time processing.  This is 
        /// not called during simulation so plugins should not do anything here that would
        /// be necessary for logon processing.
        /// </summary>
        void Starting();

        /// <summary>
        /// Called when the pGina service is shutting down, for 'stopping' time processing.  This 
        /// is not called during simulation.  Plugins should not depend on this to clean up
        /// post logon.
        /// </summary>
        void Stopping();
    }
    
    /// <summary>
    /// Plugins which wish to integrate with the pGina configuration/Plugin
    /// management UI must implement this interface 
    /// </summary>
    public interface IPluginConfiguration : IPluginBase
    {
        void Configure();
    }
    
    
    /// <summary>
    /// Plugins that want to be available for use in authentication must
    /// implement this interface.  At least one plugin
    /// must succeed for the login process to continue. 
    /// </summary>
    public interface IPluginAuthentication : IPluginBase
    {
        Types.BooleanResult AuthenticateUser(Types.SessionProperties properties);
    }

    /// <summary>
    /// Plugins that want to validate a users access (not identity per-se) must
    ///  implement this interface.  All plugins which implement this interface
    ///  must succeed for the login process to continue. 
    /// </summary>
    public interface IPluginAuthorization : IPluginBase
    {
        Types.BooleanResult AuthorizeUser(Types.SessionProperties properties);
    }

    /// <summary>
    /// Plugins that want to be involved in account management (post-auth*) 
    ///  must implement this interface.  All plugins which implement this interface
    ///  must succeed for the login process to continue.
    /// </summary>
    public interface IPluginAuthenticationGateway : IPluginBase
    {       
        /// <summary>
        /// User has been authenticated and authorized - now
        ///  is your chance to do other accounting/management before the user's login is successful.
        /// </summary>
        /// <param name="properties">Info about the session</param>
        /// <returns>Whether or not the plugin was successful.</returns>
        Types.BooleanResult AuthenticatedUserGateway(Types.SessionProperties properties);
    }          

    /// <summary>
    /// Plugins that want notification of events as they occur must implement
    /// this interface.  Note that these are notifications only - these are
    /// called from the core service in the context of the service and it's 
    /// session (i.e. not in users session, as user, etc).  Plugins which want
    /// to perform processing which requires specific context should see the 
    /// IPlugin[User|System]SessionHelper interfaces. 
    /// </summary>
    public interface IPluginEventNotifications : IPluginBase
    {        
        /// <summary>
        /// Default System session notification (as provided to pGina service
        ///  via http://msdn.microsoft.com/en-us/library/system.serviceprocess.servicebase.onsessionchange.aspx) 
        /// </summary>
        /// <param name="changeDescription">See MSDN, includes session id and change reason (login, logout etc)</param>
        /// <param name="properties">
        /// If the session is a pGina session, this is the properties instance used by the plugins at auth time.
        /// This value is null if the session is not a pGina session.
        /// </param>
        void SessionChange(SessionChangeDescription changeDescription, Types.SessionProperties properties);                
    }

    /// <summary>
    /// Plugins that want to have some persistent state between stages can implement this
    /// interface.  BeginChain will be called at the beginning of a login process and
    /// EndChain will be called when the login process completes (regardless of which stage
    /// causes the login to fail).  Plugins can store any state associated with the login in
    /// the SessionProperties object provided as a parameter.
    /// </summary>
    public interface IStatefulPlugin : IPluginBase
    {
        /// <summary>
        /// Called prior to authentication for every login.
        /// </summary>
        /// <param name="props"></param>
        void BeginChain(Types.SessionProperties props);

        /// <summary>
        /// Called at the end of a login process regardless of success or failure,
        /// and regardless of what stage caused the login to fail.
        /// </summary>
        /// <param name="props"></param>
        void EndChain(Types.SessionProperties props);
    }

    /// <summary>
    /// Plugins that want to support the change password scenario should
    /// implement this interface.
    /// </summary>
    public interface IPluginChangePassword : IPluginBase
    {
        /// <summary>
        /// Attempt to change the password.
        /// </summary>
        /// <param name="props">An object that 
        /// contains the username, domain, old and new passwords.</param>
        /// <param name="pluginInfo">An object that contains information about the
        /// results of plugins that have executed prior to this one.</param>
        /// <returns>Success/failure of the change password operation.</returns>
        Types.BooleanResult ChangePassword(Types.ChangePasswordInfo cpInfo, Types.ChangePasswordPluginActivityInfo pluginInfo);
    }
}
