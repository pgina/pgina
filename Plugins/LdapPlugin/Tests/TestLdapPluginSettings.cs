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
            settings.LdapHost = new string[] { "one.example.com", "two.example.com" };
            settings.LdapPort = 389;
            settings.UseSsl = true;
            settings.Save();
        }

        [Fact]
        public void testLoad01()
        {
            LdapPluginSettings settings = LdapPluginSettings.Load();
                
            Assert.Equal(new string[] {"one.example.com", "two.example.com"}, settings.LdapHost);
            Assert.Equal(389, settings.LdapPort);
            Assert.True(settings.UseSsl);
            Assert.Equal("", settings.ServerCertFile);
            Assert.False(settings.DoSearch);
            Assert.Equal(new string[] {}, settings.SearchContexts);
            Assert.Equal("", settings.SearchFilter);
            Assert.Equal("uid=%u,dc=example,dc=com", settings.DnPattern);
            Assert.Equal("", settings.SearchDN);
            Assert.Equal("", settings.SearchPW);
            Assert.False(settings.DoGroupAuthorization);
            Assert.Equal(new string[] {}, settings.LdapLoginGroups);
            Assert.Equal("wheel", settings.LdapAdminGroup);
        }

    }
}
