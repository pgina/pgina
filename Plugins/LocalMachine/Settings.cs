using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Settings;

namespace pGina.Plugin.LocalMachine
{
    public class Settings
    {
        private static dynamic m_settings = new DynamicSettings(PluginImpl.PluginUuid);

        static Settings()
        {            
            // Authentication step settings:
            m_settings.SetDefault("AuthenticateAsFallback", false);      // Only authenticate if no other plugins have done so yet

            // Authorization step settings
            m_settings.SetDefault("AuthzLocalAdminsOnly", false);        // Prevent non-admins
            m_settings.SetDefault("AuthzLocalGroups", new string[] { }); // Only users in these groups (in their UserInformation, not in SAM!) can be authorized
            m_settings.SetDefault("AuthzApplyToAllUsers", true);         // Authorize *all* users according to above rules, if false - non-admin/group checks are only done for users *we* authenticated            
            m_settings.SetDefault("MirrorGroupsForAuthdUsers", false);   // Load users groups from local SAM into userInfo
            
            // Gateway step settings                        
            m_settings.SetDefault("GroupCreateFailIsFAIL", true);        // Do we fail gateway if group create/add fails?            
            m_settings.SetDefault("MandatoryGroups", new string[] { });  // *All* users are added to these groups (by name)
        }

        public static dynamic Store
        {
            get { return m_settings; }            
        }
    }
}
