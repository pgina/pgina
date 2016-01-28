/*
	Copyright (c) 2013, pGina Team
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

namespace pGina.Plugin.Ldap
{
    public class Settings
    {

        public enum EncryptionMethod
        {
            NO_ENCRYPTION = 0, TLS_SSL = 1, START_TLS = 2
        }

        public static dynamic Store
        {
            get { return m_settings; }
        }
        private static dynamic m_settings;

        static Settings()
        {
            m_settings = new pGinaDynamicSettings(LdapPlugin.LdapUuid);

            // Set default values for settings (if not already set)
            m_settings.SetDefault("LdapHost", new string[] { "ldap.example.com" });
            m_settings.SetDefault("LdapPort", 389);
            m_settings.SetDefault("LdapTimeout", 10);
            m_settings.SetDefault("EncryptionMethod", (int)EncryptionMethod.NO_ENCRYPTION);
            m_settings.SetDefault("RequireCert", false);
            m_settings.SetDefault("ServerCertFile", "");            
            m_settings.SetDefault("SearchDN", "");
            m_settings.SetDefaultEncryptedSetting("SearchPW", "");
            m_settings.SetDefault("GroupDnPattern", "cn=%g,ou=Group,dc=example,dc=com");
            m_settings.SetDefault("GroupMemberAttrib", "memberUid");
            m_settings.SetDefault("Dereference", (int)System.DirectoryServices.Protocols.DereferenceAlias.Never);
            m_settings.SetDefault("UseAuthBindForAuthzAndGateway", false);

            // Authentication
            m_settings.SetDefault("AllowEmptyPasswords", false);
            m_settings.SetDefault("DnPattern", "uid=%u,dc=example,dc=com");
            m_settings.SetDefault("DoSearch", false);
            m_settings.SetDefault("SearchFilter", "");
            m_settings.SetDefault("SearchContexts", new string[] { });

            // Authorization
            m_settings.SetDefault("GroupAuthzRules", new string[] { (new GroupAuthzRule(true)).ToRegString() });
            m_settings.SetDefault("AuthzRequireAuth", false);
            m_settings.SetDefault("AuthzAllowOnError", true);
            m_settings.SetDefault("AuthzApplyToAllUsers", true); // Authorize *all* users according to above rules, if false - group checks are only done for users *we* authenticated

            // Gateway
            m_settings.SetDefault("GroupGatewayRules", new string[] { });

            // Change password
            m_settings.SetDefault("ChangePasswordAttributes",
                new string[] { 
                    new PasswordAttributeEntry { Name = "userPassword", Method = HashMethod.SHA1 }.ToRegistryString()
                }
            );
        }
    }
}
