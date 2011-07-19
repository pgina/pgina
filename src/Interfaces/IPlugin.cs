using System;
using System.Collections.Generic;

namespace pGina.Interfaces
{    
    // All plugins must implement this interface
    public interface IPluginBase
    {
        string Name { get; }
        string Description { get; }
        Guid Uuid { get; }
    }

    // Plugins which wish to integrate with the pGina configuration/Plugin
    // management UI must implement this interface
    public interface IPluginConfigurable : IPluginBase
    {        
    }

    // Plugins that want to customize what fields are shown on the login 
    //  screen must implement this interface
    public interface IPluginAuthenticationUI : IPluginBase
    {        
        void SetupUI(List<AuthenticationUI.Element> elements);
    }

    // Plugins that want to be available for use in authentication must
    // implement this interface
    public interface IPluginAuthentication : IPluginBase
    {
        bool AuthenticateUser(AuthenticationUI.Element[] values, Guid trackingToken);
    }

    // Plugins that want to be involved in account management (post-auth) 
    // must implement this interface
    public interface IPluginAuthenticationResult : IPluginBase
    {
        AuthenticationResult PrepareWayForAuthenticatedUser(AuthenticationUI.Element[] values, Guid trackingToken);        
    }
    
    // Plugins that want notification of events as they occur must implement
    // this interface.  Note that these are notifications only - these are
    // called from the core service in the context of the service and it's 
    // session (i.e. not in users session, as user, etc).  Plugins which want
    // to perform processing which requires specific context should see the 
    // IPlugin[User|System]SessionHelper interfaces.
    public interface IPluginEventNotifications : IPluginBase
    {
        // on login, logout, ... lock/disconnect etc?        
    }

    // Plugins that want to perform processing as the user should implement
    // this interface.  These plugins are loaded in the context of the users
    // session, as the user.  Note that users can stop (kill) the user session helper,
    // so *enforcement* does not belong here.
    public interface IPluginUserSessionHelper
    {        
    }

    // Plugins that want to perform processing in the users session should implement
    // this interface.  These plugins are loaded in the context of the users
    // session, as the service account that runs the pGina service.  Non-admin
    // user's cannot stop this helper (admin users can).
    public interface IPluginSystemSessionHelper
    {
    }
}
