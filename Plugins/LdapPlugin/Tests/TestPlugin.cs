using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using Xunit;
using pGina.Plugin.Ldap;
using pGina.Shared.Interfaces;
using pGina.Shared.AuthenticationUI;
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
        private List<Element> elements;
        private EditTextElement unameEl;
        private PasswordTextElement passEl;

        public TestPlugin()
        {
            // Setup the UI
            elements = new List<Element>();
            plugIn = new LdapPlugin();
            plugIn.SetupUI(elements);

            // Store the username and password elements.
            unameEl = (EditTextElement)elements[0];
            passEl = (PasswordTextElement)elements[1];

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
            unameEl.Text = "doej";
            passEl.Text = "secret";
            BooleanResult result = plugIn.AuthenticateUser(elements.ToArray(), new Guid());

            Assert.True( result.Success );
        }

        public void SetFixture(Fixture data)
        {
            // Not used, yet...   
        }
    }
}
