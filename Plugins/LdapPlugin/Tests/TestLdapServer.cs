using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.DirectoryServices.Protocols;

using Xunit;
using pGina.Plugin.Ldap;

namespace pGina.Plugin.Ldap.Tests
{
    public class TestLdapServer
    {
        private string host;
        private int port;

        public TestLdapServer()
        {
            host = "ldap.example.com";
            port = 389;
        }

        /// <summary>
        /// Test an anonymous bind.
        /// </summary>
        [Fact]
        public void TestConnect1()
        {
            LdapServer serv = new LdapServer(host, port);
            serv.Bind();
        }

        /// <summary>
        /// Test a bind with credentials.
        /// </summary>
        [Fact]
        public void TestConnect2()
        {
            LdapServer serv = new LdapServer(host, port);
            serv.Bind(new NetworkCredential("cn=Manager,dc=example,dc=com", "secret"));
        }

        /// <summary>
        /// Test a bind that should fail.
        /// </summary>
        [Fact]
        public void TestConnect3()
        {
            LdapServer serv = new LdapServer(host, port);
            Assert.Throws<LdapException>(delegate
                {
                    serv.Bind(new NetworkCredential("cn=Manager,dc=example,dc=com", "seret"));
                }
            );
        }
    }
}
