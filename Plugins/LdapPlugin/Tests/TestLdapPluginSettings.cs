using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Win32;

using Xunit;

namespace pGina.Plugin.Ldap.Tests
{
    public class TestLdapPluginSettings
    {
        public TestLdapPluginSettings()
        {
            LdapPluginSettings settings = LdapPluginSettings.Load();

            settings.LdapHost = new string[] { "192.168.51.100", "192.168.56.101" };
            settings.LdapPort = 389;
            settings.LdapTimeout = 10;
            settings.UseSsl = false;
            settings.RequireCert = true;
            settings.ServerCertFile = "";
            settings.DoSearch = true;
            settings.SearchContexts = new string[] { "ou=Stuff,dc=example,dc=com", "ou=People,dc=example,dc=com" };
            settings.SearchFilter = "(&(uid=%u)(objectClass=posixAccount))";
            settings.DnPattern = "uid=%u,dc=example,dc=com";
            settings.SearchDN = "cn=Manager,dc=example,dc=com";
            settings.SearchPW = "secret";
            settings.DoGroupAuthorization = false;
            settings.LdapLoginGroups = new string[] { };
            settings.LdapAdminGroup = "wheel";

            settings.Save();
        }

        [Fact]
        public void testLoad01()
        {
            LdapPluginSettings settings = LdapPluginSettings.Load();
                
            Assert.Equal(new string[] {"192.168.51.100", "192.168.56.101"}, settings.LdapHost);
            Assert.Equal(389, settings.LdapPort);
            Assert.Equal(10, settings.LdapTimeout);
            Assert.False(settings.UseSsl);
            Assert.True(settings.RequireCert);
            Assert.Equal("", settings.ServerCertFile);
            Assert.True(settings.DoSearch);
            Assert.Equal(new string[] { "ou=Stuff,dc=example,dc=com", "ou=People,dc=example,dc=com" }, settings.SearchContexts);
            Assert.Equal("(&(uid=%u)(objectClass=posixAccount))", settings.SearchFilter);
            Assert.Equal("uid=%u,dc=example,dc=com", settings.DnPattern);
            Assert.Equal("cn=Manager,dc=example,dc=com", settings.SearchDN);
            Assert.Equal("secret", settings.SearchPW);
            Assert.False(settings.DoGroupAuthorization);
            Assert.Equal(new string[] {}, settings.LdapLoginGroups);
            Assert.Equal("wheel", settings.LdapAdminGroup);
        }

    }
}
