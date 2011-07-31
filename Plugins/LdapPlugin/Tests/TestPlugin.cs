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
