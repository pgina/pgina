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
        [Fact]
        public void testDefaults()
        {
            LdapPluginSettings settings = LdapPluginSettings.Load();
                
            Assert.Equal(new string[] {"ldap.example.com"}, settings.LdapHost);
            Assert.Equal(389, settings.LdapPort);
            Assert.False(settings.UseSsl);
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

        [Fact]
        public void testDefaultsSave()
        {
            LdapPluginSettings settings = LdapPluginSettings.Load();
            settings.LdapPort = 222;
            settings.Save();
            settings = LdapPluginSettings.Load();

            Assert.Equal(new string[] { "ldap.example.com" }, settings.LdapHost);
            Assert.Equal(222, settings.LdapPort);
            Assert.False(settings.UseSsl);
            Assert.Equal("", settings.ServerCertFile);
            Assert.False(settings.DoSearch);
            Assert.Equal(new string[] { }, settings.SearchContexts);
            Assert.Equal("", settings.SearchFilter);
            Assert.Equal("uid=%u,dc=example,dc=com", settings.DnPattern);
            Assert.Equal("", settings.SearchDN);
            Assert.Equal("", settings.SearchPW);
            Assert.False(settings.DoGroupAuthorization);
            Assert.Equal(new string[] { }, settings.LdapLoginGroups);
            Assert.Equal("wheel", settings.LdapAdminGroup);
        }

    }
}
