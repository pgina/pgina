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

namespace pGina.Plugin.MySQLAuth
{
    class Settings
    {
        public enum HashEncoding { HEX = 0, BASE_64 = 1 };

        private static dynamic m_settings = new pGina.Shared.Settings.pGinaDynamicSettings(PluginImpl.PluginUuid);
        public static dynamic Store
        {
            get { return m_settings; }
        }

        static Settings()
        {
            m_settings.SetDefault("Host", "localhost");
            m_settings.SetDefault("Port", 3306);
            m_settings.SetDefault("UseSsl", false);
            m_settings.SetDefault("User", "pgina_user");
            m_settings.SetDefaultEncryptedSetting("Password", "secret");
            m_settings.SetDefault("Database", "account_db");

            // User table
            m_settings.SetDefault("Table", "users");
            m_settings.SetDefault("HashEncoding", (int)HashEncoding.HEX);
            m_settings.SetDefault("UsernameColumn", "user_name");
            m_settings.SetDefault("HashMethodColumn", "hash_method");
            m_settings.SetDefault("PasswordColumn", "password");
            m_settings.SetDefault("UserTablePrimaryKeyColumn", "user_id");

            // Group table
            m_settings.SetDefault("GroupTableName", "groups");
            m_settings.SetDefault("GroupNameColumn", "group_name");
            m_settings.SetDefault("GroupTablePrimaryKeyColumn", "group_id");

            // User-Group table
            m_settings.SetDefault("UserGroupTableName", "usergroup");
            m_settings.SetDefault("UserForeignKeyColumn", "user_id");
            m_settings.SetDefault("GroupForeignKeyColumn", "group_id");

            // Authz Settings
            m_settings.SetDefault("GroupAuthzRules", new string[] { (new GroupAuthzRule(true)).ToRegString() } );
            m_settings.SetDefault("AuthzRequireMySqlAuth", false);

            // Gateway settings
            m_settings.SetDefault("GroupGatewayRules", new string[] { });
        }
    }
}
