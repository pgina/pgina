using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pGina.Plugin.Ldap;
using System.DirectoryServices.Protocols;

using NUnit.Framework;
using pGina.Shared.Types;

namespace pGina.Plugin.Ldap.Test
{
    [TestFixture]
    public class LdapTests
    {
        static dynamic m_settings;

        static readonly string[] host = { "192.168.56.101" };
        static readonly int port = 389;
        static readonly bool useSsl = false;
        static readonly bool validateCert = false;
        static readonly string searchDN = "";   // anonymous bind
        static readonly string searchPW = "";

        static readonly Guid BogusSessionId = new Guid("9B599C0B-08C4-43F0-A2DB-42C73A3C59F5");
        
        private LdapPlugin m_plugin;
        private SessionProperties m_props;

        [TestFixtureSetUp]
        public void InitFixture()
        {
            pGina.Shared.Logging.Logging.Init();

            m_settings = new pGina.Shared.Settings.pGinaDynamicSettings(Ldap.LdapPlugin.LdapUuid);

            m_plugin = new LdapPlugin();
            m_plugin.Starting();
        }

        [TestFixtureTearDown]
        public void TearDownFixture()
        {
            m_plugin.Stopping();
        }

        [SetUp]
        public void InitTest()
        {
            // Default test settings, reset for each test

            m_settings.LdapHost = host;
            m_settings.LdapPort = port;
            m_settings.LdapTimeout = 10;
            m_settings.UseSsl = useSsl;
            m_settings.RequireCert = validateCert;
            m_settings.SearchDN = searchDN;
            m_settings.SetEncryptedSetting("SearchPW", searchPW);
            m_settings.GroupDnPattern = "cn=%g,ou=Group,dc=example,dc=com";
            m_settings.GroupMemberAttrib = "memberUid";

            // Authentication
            m_settings.AllowEmptyPasswords = false;
            m_settings.DnPattern = "uid=%u,ou=People,dc=example,dc=com";
            m_settings.DoSearch = false;
            m_settings.SearchFilter = "";
            m_settings.SearchContexts = new string[] { };

            // Authorization
            m_settings.GroupAuthzRules = new string[] { (new GroupAuthzRule(true)).ToRegString() };
            m_settings.AuthzRequireAuth = false;
            m_settings.AuthzAllowOnError = true;

            // Gateway
            m_settings.GroupGatewayRules = new string[] { };

            // Set up session props
            m_props = new SessionProperties(BogusSessionId);
            UserInformation userInfo = new UserInformation();
            m_props.AddTrackedSingle<UserInformation>(userInfo);
            userInfo.Username = "kirkj";
            userInfo.Password = "secret";
            PluginActivityInformation actInfo = new PluginActivityInformation();
            m_props.AddTrackedSingle<PluginActivityInformation>(actInfo);
        }

        [TearDown]
        public void TearDown()
        {

        }

        // A simple authentication, no frills.
        [Test] public void AuthSimple()
        {
            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthenticateUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(result.Success, result.Message);
        }

        // Disallow empty passwords (default)
        [Test]
        public void AuthEmpty()
        {
            // Modify settings
            UserInformation userInfo = m_props.GetTrackedSingle<UserInformation>();
            userInfo.Password = "";

            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthenticateUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(!result.Success, result.Message);
            Assert.That(result.Message, Is.EqualTo("Authentication failed due to empty password."));
        }
        
        [Test]
        public void AuthSimpleFail()
        {
            // Modify settings
            UserInformation userInfo = m_props.GetTrackedSingle<UserInformation>();
            userInfo.Password = "Wrong password";
            
            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthenticateUser(m_props);
            m_plugin.EndChain(m_props);
            
            Assert.That(!result.Success, result.Message);
        }

        // An authentication where we need to search for the DN
        [Test]
        public void AuthSearch()
        {
            m_settings.DoSearch = true;
            m_settings.SearchFilter = "(uid=%u)";
            m_settings.SearchContexts = new string[] { "ou=People,dc=example,dc=com" };

            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthenticateUser(m_props);
            m_plugin.EndChain(m_props);
            
            Assert.That(result.Success, result.Message);
        }

        // A failed authentication where we need to search for the DN
        [Test]
        public void AuthSearchFail()
        {
            // Modify settings
            UserInformation userInfo = m_props.GetTrackedSingle<UserInformation>();
            userInfo.Password = "Wrong password";

            m_settings.DoSearch = true;
            m_settings.SearchFilter = "(uid=%u)";
            m_settings.SearchContexts = new string[] { "ou=People,dc=example,dc=com" };

            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthenticateUser(m_props);
            m_plugin.EndChain(m_props);
            
            Assert.That(!result.Success, result.Message);
        }

        // Unable to find the DN
        [Test]
        public void AuthSearchFailDnSearch()
        {
            m_settings.DoSearch = true;
            m_settings.SearchFilter = "(uid=%u)";
            m_settings.SearchContexts = new string[] { "ou=Group,dc=example,dc=com" };

            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthenticateUser(m_props);
            m_plugin.EndChain(m_props);
            
            Assert.That(!result.Success, result.Message);
            Assert.That(result.Message, Is.EqualTo("Unable to determine the user's LDAP DN for authentication.") );
        }

        // Deny when LDAP auth fails
        [Test]
        public void AuthzDenyAuth()
        {
            m_settings.AuthzRequireAuth = true;

            m_plugin.BeginChain(m_props);
            PluginActivityInformation actInfo = m_props.GetTrackedSingle<PluginActivityInformation>();
            actInfo.AddAuthenticateResult(LdapPlugin.LdapUuid, new BooleanResult { Success = false });
            BooleanResult result = m_plugin.AuthorizeUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(!result.Success, result.Message);
            Assert.That(result.Message, Is.EqualTo("Deny because LDAP authentication failed."));
        }

        // Deny when LDAP auth doesn't execute
        [Test]
        public void AuthzDenyAuthNoExecute()
        {
            m_settings.AuthzRequireAuth = true;

            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthorizeUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(!result.Success, result.Message);
            Assert.That(result.Message, Is.EqualTo("Deny because LDAP auth did not execute, and configured to require LDAP auth."));
        }

        [Test]
        public void AuthzAllowGroup()
        {
            // Allow by default rule (not a member of "good")
            m_settings.GroupAuthzRules = new string[] {
                new GroupAuthzRule("good", GroupRule.Condition.MEMBER_OF, true).ToRegString(),
                (new GroupAuthzRule(true)).ToRegString()
            };

            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthorizeUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(result.Success, result.Message);
        }

        [Test]
        public void AuthzAllowGroup01()
        {
            // Allow because I'm not a member of group "good"
            m_settings.GroupAuthzRules = new string[] {
                new GroupAuthzRule("good", GroupRule.Condition.NOT_MEMBER_OF, true).ToRegString(),
                (new GroupAuthzRule(true)).ToRegString()
            };

            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthorizeUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(result.Success, result.Message);
        }

        [Test]
        public void AuthzDenyGroup()
        {
            // Deny because I'm a member of group "bad"
            m_settings.GroupAuthzRules = new string[] {
                new GroupAuthzRule("bad", GroupRule.Condition.MEMBER_OF, false).ToRegString(),
                (new GroupAuthzRule(true)).ToRegString()
            };

            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthorizeUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(!result.Success, result.Message);
        }
    }
}
