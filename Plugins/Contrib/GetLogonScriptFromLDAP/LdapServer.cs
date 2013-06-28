/*
    Copyright (c) 2011, pGina Team
    All rights reserved.
    Adapted to the LogonScript plugin by Florian Rohmer (2013).

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
using System.DirectoryServices.Protocols;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

// For registry access
using Microsoft.Win32;

using log4net;
using pGina.Shared.Types;
using pGina.Shared.Settings;

namespace pGina.Plugin.LogonScriptFromLDAP
{
    public class LdapServer : IDisposable
    {
        /// <summary>
        /// logger for LdapServer
        /// </summary>
        private ILog server_logger = LogManager.GetLogger("LogonScript_LdapServer");
  
        /// <summary>
        /// The connection object
        /// </summary>
        private LdapConnection m_conn = null;

        /// <summary>
        /// The server identification (host,port).
        /// </summary>
        private LdapDirectoryIdentifier m_serverIdentifier;

        /// <summary>
        /// Whether or not to use SSL
        /// Boolean comes from LDAP Plugin configuration
        /// </summary>
        private bool m_useSsl ;

        /// <summary>
        /// Whether or not to verify the SSL certificate
        /// Boolean comes from LDAP Plugin configuration
        /// </summary>
        private bool m_verifyCert ; 

        /// <summary>
        /// The SSL certificate to verify against (if required)
        /// Comes from LDAP Plugin configuration
        /// </summary>
        private X509Certificate2 m_cert ;

        /// <summary>
        /// The number of seconds to wait for a connection before giving up.
        /// Comes from LDAP Plugin configuration
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Name (found on LDAP) of the script file to find on the server given in the configuration window
        /// </summary>
        private string scriptName;

        /// <summary>
        /// Path leading to the LdapPlugin registry
        /// </summary>
        private string LdapPluginPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\pGina3\Plugins\0f52390b-c781-43ae-bd62-553c77fa4cf7";

        public LdapServer()
        {      
            m_conn = null;
            m_cert = null;
            Timeout = (int)Registry.GetValue(LdapPluginPath, "LdapTimeout", 10);
            m_useSsl = Convert.ToBoolean(Registry.GetValue(LdapPluginPath, "UseSsl", null).ToString());
            m_verifyCert = Convert.ToBoolean(Registry.GetValue(LdapPluginPath, "RequireCert", null));
            string certFile = (string) Registry.GetValue(LdapPluginPath, "ServerCertFile", null);

            if (m_useSsl && m_verifyCert)
            {
                if (!string.IsNullOrEmpty(certFile) && File.Exists(certFile))
                {
                    server_logger.DebugFormat("Loading server certificate: {0}", certFile);
                    m_cert = new X509Certificate2(certFile);
                }
                server_logger.DebugFormat("Certificate file not provided or not found, will validate against Windows store.", certFile);
            }

            string[] ldapHost =
                (string[])Registry.GetValue(LdapPluginPath, "LdapHost", null);

            int ldapPort =
                (int)Registry.GetValue(LdapPluginPath, "LdapPort", null);

            m_serverIdentifier = new LdapDirectoryIdentifier(ldapHost, ldapPort, false, false);

            server_logger.DebugFormat("Initializing LdapServer host(s): [{0}], port: {1}, useSSL = {2}, verifyCert = {3}",
            string.Join(", ", ldapHost), ldapPort, m_useSsl, m_verifyCert);

            this.Connect();
        }

        private void Connect()
        {
            // Are we re-connecting?  If so, close the previous connection.
            if (m_conn != null)
            {
                this.Close();
            }

            m_conn = new LdapConnection(m_serverIdentifier);
            m_conn.Timeout = new System.TimeSpan(0, 0, Timeout);
            server_logger.DebugFormat("Timeout set to {0} seconds.", Timeout);
            m_conn.SessionOptions.ProtocolVersion = 3;
            m_conn.SessionOptions.SecureSocketLayer = m_useSsl;

            if (m_useSsl)
                m_conn.SessionOptions.VerifyServerCertificate = this.VerifyCert;
        }


        /// <summary>
        /// This is the verify certificate callback method used when initially binding to the
        /// LDAP server.  This manages all certificate validation.
        /// </summary>
        /// <param name="conn">The LDAP connection.</param>
        /// <param name="cert">The server's certificate</param>
        /// <returns>true if verification succeeds, false otherwise.</returns>
        private bool VerifyCert(LdapConnection conn, X509Certificate cert)
        {
            server_logger.Debug("VerifyCert(...)");
            server_logger.DebugFormat("Verifying certificate from host: {0}", conn.SessionOptions.HostName);

            // Convert to X509Certificate2
            X509Certificate2 serverCert = new X509Certificate2(cert);

            // If we don't need to verify the cert, the verification succeeds
            if (!m_verifyCert)
            {
                server_logger.Debug("Server certificate accepted without verification.");
                return true;
            }

            // If the certificate is null, then we verify against the machine's/user's certificate store
            if (m_cert == null)
            {
                server_logger.Debug("Verifying server cert with Windows store.");

                // We set the RevocationMode to NoCheck because most custom (self-generated) CAs
                // do not work properly with revocation lists.  This is slightly less secure, but
                // the most common use case for this plugin probably doesn't rely on revocation
                // lists.
                X509ChainPolicy policy = new X509ChainPolicy()
                {
                    RevocationMode = X509RevocationMode.NoCheck
                };

                // Create a validator using the policy
                X509CertificateValidator validator = X509CertificateValidator.CreatePeerOrChainTrustValidator(false, policy);
                try
                {
                    validator.Validate(serverCert);

                    // If we get here, validation succeeded.
                    server_logger.Debug("Server certificate verification succeeded.");
                    return true;
                }
                catch (SecurityTokenValidationException e)
                {
                    server_logger.ErrorFormat("Server certificate validation failed: {0}", e.Message);
                    return false;
                }
            }
            else
            {
                server_logger.Debug("Validating server certificate with provided certificate.");

                // Verify against the provided cert by comparing the thumbprint
                bool result = m_cert.Thumbprint == serverCert.Thumbprint;
                if (result) server_logger.Debug("Server certificate validated.");
                else server_logger.Debug("Server certificate validation failed.");
                return result;
            }
        }

        /// <summary>
        /// closes current connection
        /// </summary>
        public void Close()
        {
            if (m_conn != null)
            {
                server_logger.DebugFormat("Closing LDAP connection to {0}.", m_conn.SessionOptions.HostName);
                m_conn.Dispose();
                m_conn = null;
            }
        }

        /// <summary>
        /// closes current connection
        /// </summary>
        public void Dispose()
        {
            this.Close();
        }

        /// <summary>
        /// Attempts to authenticate the user by binding to the LDAP server.
        /// Also retrieves the name of the script file.
        /// </summary>
        public BooleanResult Authenticate(string uname, string password)
        {
            // Check for empty password. If configured to do so, we fail on 
            // empty passwords.
            bool allowEmpty = Convert.ToBoolean(
                    Registry.GetValue(LdapPluginPath, "AllowEmptyPasswords", null));

            if (!allowEmpty && string.IsNullOrEmpty(password))
            {
                server_logger.Info("Authentication failed due to empty password.");
                return new BooleanResult { Success = false, Message = "Authentication failed due to empty password." };
            }

            // Get the user's DN and script name
            string[] userDnAndScript = GetUserDnAndScriptName(uname);

            if (userDnAndScript == null)
            {
                server_logger.DebugFormat("Retrieving Dn and/or script name didn't work correctly.");
                return new BooleanResult { Success = false, Message = "Please make sure the configuration is correct. Retrieving Dn and/or script name didn't work correctly." };
            }

            string userDN = userDnAndScript[0];
            if (userDN == null)
            {
                server_logger.ErrorFormat("Unable to determine DN for: {0}", uname);
                return new BooleanResult { Success = false, Message = "Unable to determine the user's LDAP DN for authentication." };
            }

            this.scriptName = userDnAndScript[1];
            if (scriptName == null)
            {
                server_logger.ErrorFormat("Unable to find the script name for the attribute given. Make sure the config is correct");
                return new BooleanResult { Success = false, Message = "Impossible to find script name. Make sure the config is correct." };
            }

            // If we've got a userDN, attempt to authenticate the user
            // Attempt to bind with the user's LDAP credentials
            server_logger.DebugFormat("Attempting to bind with DN {0}", userDN);
            NetworkCredential ldapCredential = new NetworkCredential(userDN, password);

            try
            {
                this.Bind(ldapCredential);
            }
            catch (LdapException e)
            {
                // 49 is invalid credentials
                if (e.ErrorCode == 49)
                {
                    server_logger.ErrorFormat("LDAP bind failed: invalid credentials.");
                    return new BooleanResult { Success = false, Message = "Authentication via LDAP failed. Invalid credentials." };
                }

                // Let caller handle other kinds of exceptions
                throw;
            }

            // If we get here, the authentication was successful, we're done!
            server_logger.DebugFormat("LDAP DN {0} successfully bound to server, return success", ldapCredential.UserName);
            return new BooleanResult { Success = true };
        } 
        

        /// <summary>
        /// Tries to bind to the server anonymously. 
        /// Throws LdapException if the bind fails.
        /// </summary>
        public void Bind()
        {
            if (m_conn == null)
                throw new LdapException("Bind attempted when server is not connected.");

            server_logger.DebugFormat("Attempting anonymous bind", m_conn.SessionOptions.HostName);
            m_conn.AuthType = AuthType.Anonymous;
            m_conn.Credential = null;
            try
            {
                m_conn.Bind();
                server_logger.DebugFormat("Successful bind to {0}", m_conn.SessionOptions.HostName);
            }
            catch (LdapException e)
            {
                server_logger.ErrorFormat("LdapException: {0} {1}", e.Message, e.ServerErrorMessage);
                throw e;
            }
            catch (InvalidOperationException e)
            {
                // This shouldn't happen, but log it and re-throw
                server_logger.ErrorFormat("InvalidOperationException: {0}", e.Message);
                throw e;
            }
        }

        public void BindForSearch()
        {
            string searchDn = (string)Registry.GetValue(LdapPluginPath, "SearchDN", null);
            string searchPw = (string)Registry.GetValue(LdapPluginPath, "SearchPW", null);

            if (string.IsNullOrEmpty(searchDn))
                // Bind anonymously
                this.Bind();
            else
                // Bind with credentials
                this.Bind(new NetworkCredential(searchDn, searchPw));
        }

        /// <summary>
        /// Tries to bind to the LDAP server with the given credentials. This uses
        /// basic authentication. Throws LdapException if the bind fails.
        /// </summary>
        /// <param name="creds">The credentials to use when binding.</param>
        public void Bind(NetworkCredential creds)
        {
            if (m_conn == null)
                throw new LdapException("Bind attempted when server is not connected.");

            server_logger.DebugFormat("Attempting bind as {0}", creds.UserName);

            // we choose the basic authentication type
            m_conn.AuthType = AuthType.Basic;

            try
            {
                m_conn.Bind(creds);
                server_logger.DebugFormat("Successful bind to {0} as {1}", m_conn.SessionOptions.HostName, creds.UserName);
            }
            catch (LdapException e)
            {
                server_logger.ErrorFormat("LdapException: {0} {1}", e.Message, e.ServerErrorMessage);
                throw e;
            }
            catch (InvalidOperationException e)
            {
                // This shouldn't happen, but log it and re-throw
                server_logger.ErrorFormat("InvalidOperationException: {0}", e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Does a search in the subtree at searchBase using the filter provided and 
        /// returns the DN of the first match.
        /// </summary>
        /// <param name="searchBase">The DN of the root of the subtree for the search (search context).</param>
        /// <param name="filter">The search filter.</param>
        /// <returns>The DN of the first match, or null if no matches are found.</returns>
        public string FindFirstDN(string searchBase, string filter)
        {
            SearchRequest req = new SearchRequest(searchBase, filter, System.DirectoryServices.Protocols.SearchScope.Subtree, null);
            SearchResponse resp = (SearchResponse)m_conn.SendRequest(req);

            if (resp.Entries.Count > 0)
            {
                return resp.Entries[0].DistinguishedName;
            }

            return null;
        }

        /// <summary>
        /// Finds on the LDAP Server the value of the script name 
        /// given in the configuration window
        /// </summary>
        public string RetrieveScriptName(string searchBase, string filter)
        {
            SearchRequest req = new SearchRequest(searchBase, filter, System.DirectoryServices.Protocols.SearchScope.Subtree, null);
            SearchResponse resp = (SearchResponse)m_conn.SendRequest(req);

            try
            {
                foreach (SearchResultEntry entry in resp.Entries)
                {
                    string attributeToFind = (string) Settings.Store.Attribute;

                    DirectoryAttribute scriptName = entry.Attributes[attributeToFind];

                    return (string)scriptName[0];
                }
            }
            catch (NullReferenceException e)
            {
                server_logger.DebugFormat("Attribute field does not exist.");
            }

            return null; 
        }

        /// <summary>
        /// decides if we have to search for dn or not
        /// </summary>
        /// <returns>a string[] of two elements. 
        /// First one is dn, second one is the name of the script file.</returns>
        public string[] GetUserDnAndScriptName(string uname)
        {
            bool doSearch = Convert.ToBoolean((Registry.GetValue(LdapPluginPath, "DoSearch", null)).ToString());

            if (doSearch)
            {
                return FindUserDnAndScriptName(uname);
            }
            else
            {
                string dn = CreateUserDN(uname);
                string[][] filterAndContexts = GetFilterAndContext(uname);
                string scriptName = RetrieveScriptName(filterAndContexts[1][0], filterAndContexts[0][0]);

                return new string[] { dn, scriptName };
            }
        }

        /// <summary>
        /// Attempts to find the DN for the user by searching a set of LDAP trees.
        /// The base DN for each of the trees is retrieved from SearchContexts in the Registry.
        /// The search filter is taken from the Registry too. 
        /// </summary>
        /// <returns>string[0] : The DN of the first object found, or null if searches fail,
        /// and string[1] : the name of the script file.</returns>
        private string[] FindUserDnAndScriptName(string uname)
        {
            // Attempt to bind in order to do the search
            this.BindForSearch();

            // retrieving filter and context
            string[][] filterAndContexts = GetFilterAndContext(uname);
            string filter = filterAndContexts[0][0];
            string[] contexts = filterAndContexts[1];

            foreach (string context in contexts)
            {
                server_logger.DebugFormat("Searching context: {0}", context);
                string dn = null;
                string scriptName = null;
                try
                {
                    dn = this.FindFirstDN(context, filter);
                    scriptName = RetrieveScriptName(context, filter);
                }
                catch (DirectoryOperationException e)
                {
                    server_logger.ErrorFormat("DirectoryOperationException: {0}", e.Message);
                }

                return new string[] { dn, scriptName }; 
            }

            // shouldn't be reached
            return new string[] { null, null };
        }

        /// <summary>
        /// Creates filter and context that are required to find the script name and DN (if needed) 
        /// </summary>
        /// <returns> a string[][]. string[0][0] contains the filter
        /// string[1] contains contexts</returns>
        private string[][] GetFilterAndContext(string uname)
        {
            string filter = CreateSearchFilter(uname);

            server_logger.DebugFormat("Searching for DN using filter {0}", filter);

            string[] contexts =
                   (string[])Registry.GetValue(LdapPluginPath, "SearchContexts", null);

            return new string[][] { new string[]{filter}, contexts } ;
        }

        /// <summary>
        /// This generates the DN for the user assuming that a pattern has
        /// been provided.  This assumes DnPattern in LDAP Authentication Plugin Registry 
        /// has a valid DN pattern.
        /// </summary>
        /// <returns>A DN that can be used for binding with LDAP server.</returns>
        private string CreateUserDN(string uname)
        {
            string dn = (string)Registry.GetValue(LdapPluginPath, "DnPattern", null);

            // Replace the username
            dn = Regex.Replace(dn, @"\%u", uname);

            return dn;
        }

        /// <summary>
        /// This generates the search filter to be used when searching for the DN
        /// </summary>
        /// <returns>A search filter.</returns>
        private string CreateSearchFilter(string uname)
        {
            string searchFilter = (string)Registry.GetValue(LdapPluginPath, "SearchFilter", null);

            // Replace the username
            searchFilter = Regex.Replace(searchFilter, @"\%u", uname);

            return searchFilter;
        }

        /// <summary>
        /// returns the name of the script file
        /// method should be used by PluginImpl.
        /// </summary>
        public string GetScriptName()
        {
            return scriptName;
        }

    }
}
