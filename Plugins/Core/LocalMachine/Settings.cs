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

using pGina.Shared.Settings;

namespace pGina.Plugin.LocalMachine
{
    public class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);

        static Settings()
        {            
            // Authentication step settings:
            m_settings.SetDefault("AlwaysAuthenticate", true);           // Auth regardless of other plugins auth step

            // Authorization step settings
            m_settings.SetDefault("AuthzLocalAdminsOnly", false);        // Prevent non-admins
            m_settings.SetDefault("AuthzLocalGroupsOnly", false);        // Require group membership in following list
            m_settings.SetDefault("AuthzLocalGroups", new string[] { }); // Only users in these groups (in their UserInformation, not in SAM!) can be authorized
            m_settings.SetDefault("AuthzApplyToAllUsers", true);         // Authorize *all* users according to above rules, if false - non-admin/group checks are only done for users *we* authenticated            
            m_settings.SetDefault("MirrorGroupsForAuthdUsers", true);    // Load users groups from local SAM into userInfo
            
            // Gateway step settings                        
            m_settings.SetDefault("GroupCreateFailIsFail", true);        // Do we fail gateway if group create/add fails?            
            m_settings.SetDefault("MandatoryGroups", new string[] { });  // *All* users are added to these groups (by name)
            m_settings.SetDefault("RemoveProfiles", false);              // Do we remove accounts/profiles after logout?
            m_settings.SetDefault("ScramblePasswords", false);           // Do we scramble users passwords after logout?
            m_settings.SetDefault("ScramblePasswordsWhenLMAuthFails", true); // Only scramble when LM fails or doesnt execute
            m_settings.SetDefault("ScramblePasswordsExceptions", new string[] { });

            // Cleanup thread settings (not user configurable)
            m_settings.SetDefault("CleanupUsers", new string[] { });     // List of principal names we must cleanup!
            m_settings.SetDefault("BackgroundTimerSeconds", 60);         // How often we look to cleanup
        }

        public static dynamic Store
        {
            get { return m_settings; }            
        }
    }
}
