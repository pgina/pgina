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
using System.IO;
using System.Reflection;

using Xunit;
using pGina.Plugin.Ldap;
using pGina.Shared.Interfaces;
using pGina.Shared.Settings;
using pGina.Shared.Types;

namespace pGina.Plugin.Ldap.Tests
{

    public class Fixture
    {
        public Fixture()
        {
            pGina.Shared.Logging.Logging.Init();
        }
    }

    public class TestPlugin :IUseFixture<Fixture>
    {
        private LdapPlugin plugIn;
        private Shared.Types.SessionProperties properties = new Shared.Types.SessionProperties(Guid.NewGuid());
        private Shared.Types.UserInformation userInfo = new Shared.Types.UserInformation();

        public TestPlugin()
        {
            plugIn = new LdapPlugin();

            // Add a UserInformation object as pGina would            
            properties.AddTrackedSingle<Shared.Types.UserInformation>(userInfo);
            
            dynamic settings = new DynamicSettings(LdapPlugin.LdapUuid);

            settings.LdapHost = new string[] { "192.168.51.100", "192.168.56.101" };
            settings.LdapPort = 636;
            settings.UseSsl = true;
            settings.RequireCert = true;
            settings.ServerCertFile = @"c:\slapd.cer";
            settings.DoSearch = true;
            settings.SearchContexts = new string[] { "ou=Stuff,dc=example,dc=com", "ou=People,dc=example,dc=com" };
            settings.SearchFilter = "(&(uid=%u)(objectClass=posixAccount))";
            settings.DnPattern = "uid=%u,dc=example,dc=com";
            settings.SearchDN = "cn=search user,dc=example,dc=com";
            settings.SearchPW = "secret";
        }

        [Fact]
        public void TestLogin01()
        {
            // Test a basic login.
            userInfo.Username = "doej";
            userInfo.Password = "secret";
            BooleanResult result = plugIn.AuthenticateUser(properties);

            Assert.True( result.Success );
        }

        public void SetFixture(Fixture data)
        {
            // Not used, yet...   
        }
    }
}
