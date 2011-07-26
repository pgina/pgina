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
