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
            host = "192.168.56.101";
            port = 389;
        }

        /// <summary>
        /// Test an anonymous bind.
        /// </summary>
        [Fact]
        public void TestConnect1()
        {
            LdapServer serv = new LdapServer(new string[] {host}, port, false, false, null);
            serv.Connect(10);
            serv.Bind();
        }

        /// <summary>
        /// Test a bind with credentials.
        /// </summary>
        [Fact]
        public void TestConnect2()
        {
            LdapServer serv = new LdapServer(new string[] {host}, port, false, false, null);
            serv.Connect(10);
            serv.Bind(new NetworkCredential("cn=Manager,dc=example,dc=com", "secret"));
        }

        /// <summary>
        /// Test a bind that should fail.
        /// </summary>
        [Fact]
        public void TestConnect3()
        {
            LdapServer serv = new LdapServer(new string[]{host}, port, false, false, null);
            serv.Connect(10);
            Assert.Throws<LdapException>(delegate
                {
                    serv.Bind(new NetworkCredential("cn=Manager,dc=example,dc=com", "seret"));
                }
            );
        }
    }
}
