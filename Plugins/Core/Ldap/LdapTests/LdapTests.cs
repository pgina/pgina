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
        static readonly string[] host = { "192.168.59.1" };
        static readonly int port = 389;
        static readonly Settings.EncryptionMethod encMethod = Settings.EncryptionMethod.NO_ENCRYPTION;
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

           // m_settings = new pGina.Shared.Settings.pGinaDynamicSettings(Ldap.LdapPlugin.LdapUuid);

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

            Settings.Store.LdapHost = host;
            Settings.Store.LdapPort = port;
            Settings.Store.LdapTimeout = 10;
            Settings.Store.EncryptionMethod = (int)encMethod;
            Settings.Store.RequireCert = validateCert;
            Settings.Store.SearchDN = searchDN;
            Settings.Store.SetEncryptedSetting("SearchPW", searchPW);
            Settings.Store.GroupDnPattern = "cn=%g,ou=Group,dc=example,dc=com";
            Settings.Store.GroupMemberAttrib = "memberUid";
            Settings.Store.UseAuthBindForAuthzAndGateway = false;

            // Authentication
            Settings.Store.AllowEmptyPasswords = false;
            Settings.Store.DnPattern = "uid=%u,ou=People,dc=example,dc=com";
            Settings.Store.DoSearch = false;
            Settings.Store.SearchFilter = "";
            Settings.Store.SearchContexts = new string[] { };

            // Authorization
            Settings.Store.GroupAuthzRules = new string[] { (new GroupAuthzRule(true)).ToRegString() };
            Settings.Store.AuthzRequireAuth = false;
            Settings.Store.AuthzAllowOnError = true;

            // Gateway
            Settings.Store.GroupGatewayRules = new string[] { };

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
            Settings.Store.DoSearch = true;
            Settings.Store.SearchFilter = "(uid=%u)";
            Settings.Store.SearchContexts = new string[] { "ou=People,dc=example,dc=com" };

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

            Settings.Store.DoSearch = true;
            Settings.Store.SearchFilter = "(uid=%u)";
            Settings.Store.SearchContexts = new string[] { "ou=People,dc=example,dc=com" };

            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthenticateUser(m_props);
            m_plugin.EndChain(m_props);
            
            Assert.That(!result.Success, result.Message);
        }

        // Unable to find the DN
        [Test]
        public void AuthSearchFailDnSearch()
        {
            Settings.Store.DoSearch = true;
            Settings.Store.SearchFilter = "(uid=%u)";
            Settings.Store.SearchContexts = new string[] { "ou=Group,dc=example,dc=com" };

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
            Settings.Store.AuthzRequireAuth = true;

            m_plugin.BeginChain(m_props);
            PluginActivityInformation actInfo = m_props.GetTrackedSingle<PluginActivityInformation>();
            actInfo.AddAuthenticateResult(LdapPlugin.LdapUuid, new BooleanResult { Success = false });
            BooleanResult result = m_plugin.AuthorizeUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(!result.Success, result.Message);
            Assert.That(result.Message, Is.EqualTo("Deny because LDAP authentication failed, or did not execute."));
        }

        // Deny when LDAP auth doesn't execute
        [Test]
        public void AuthzDenyAuthNoExecute()
        {
            Settings.Store.AuthzRequireAuth = true;

            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthorizeUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(!result.Success, result.Message);
            Assert.That(result.Message, Is.EqualTo("Deny because LDAP authentication failed, or did not execute."));
        }

        [Test]
        public void AuthzAllowGroup()
        {
            // Allow by default rule (not a member of "good")
            Settings.Store.GroupAuthzRules = new string[] {
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
            Settings.Store.GroupAuthzRules = new string[] {
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
            Settings.Store.GroupAuthzRules = new string[] {
                new GroupAuthzRule("bad", GroupRule.Condition.MEMBER_OF, false).ToRegString(),
                (new GroupAuthzRule(true)).ToRegString()
            };

            m_plugin.BeginChain(m_props);
            BooleanResult result = m_plugin.AuthorizeUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(!result.Success, result.Message);
        }

        [Test]
        public void AuthzUseAuthBind()
        {
            // Allow because I'm not a member of group "good"
            Settings.Store.GroupAuthzRules = new string[] {
                new GroupAuthzRule("good", GroupRule.Condition.NOT_MEMBER_OF, true).ToRegString(),
                (new GroupAuthzRule(true)).ToRegString()
            };
            Settings.Store.UseAuthBindForAuthzAndGateway = true;

            m_plugin.BeginChain(m_props);
            BooleanResult authResult = m_plugin.AuthenticateUser(m_props);
            Assert.That(authResult.Success);
            BooleanResult authzResult = m_plugin.AuthorizeUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(authzResult.Success, authzResult.Message);
        }

        [Test]
        public void AuthzUseAuthBindFail()
        {
            // Allow because I'm not a member of group "good"
            Settings.Store.GroupAuthzRules = new string[] {
                new GroupAuthzRule("good", GroupRule.Condition.NOT_MEMBER_OF, true).ToRegString(),
                (new GroupAuthzRule(true)).ToRegString()
            };
            Settings.Store.UseAuthBindForAuthzAndGateway = true;
            UserInformation userInfo = m_props.GetTrackedSingle<UserInformation>();
            userInfo.Password = "WrongPassword";

            m_plugin.BeginChain(m_props);
            BooleanResult authResult = m_plugin.AuthenticateUser(m_props);
            Assert.That(!authResult.Success);
            BooleanResult authzResult = m_plugin.AuthorizeUser(m_props);
            m_plugin.EndChain(m_props);

            Assert.That(!authzResult.Success, authzResult.Message);
        }
    }
}
