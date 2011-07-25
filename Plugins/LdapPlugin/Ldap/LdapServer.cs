using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.DirectoryServices.Protocols;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace pGina.Plugin.Ldap
{
    public class LdapServer : IDisposable
    {
        private log4net.ILog m_logger = log4net.LogManager.GetLogger("LdapServer");
        
        /// <summary>
        /// The connection object.
        /// </summary>
        private LdapConnection m_conn = null;

        /// <summary>
        /// The server identification (host,port)
        /// </summary>
        private LdapDirectoryIdentifier m_serverIdentifier;

        /// <summary>
        /// Whether or not to use SSL
        /// </summary>
        private bool m_useSsl;

        /// <summary>
        /// Whether or not to verify the SSL certificate
        /// </summary>
        private bool m_verifyCert;

        /// <summary>
        /// The SSL certificate to verify against (if required)
        /// </summary>
        private X509Certificate2 m_cert;

        /// <summary>
        /// The number of seconds to wait for a connection before giving up.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Private constructor, sets defaults for member variables.
        /// </summary>
        private LdapServer()
        {
            m_conn = null;
            m_serverIdentifier = null;
            m_useSsl = false;
            m_cert = null;
        }

        /// <summary>
        /// Create a connection to the LDAP server at the given host and port.
        /// This creates a connection that does not use SSL.
        ///</summary>
        /// <param name="host">The FQDN or IP of the LDAP host.</param>
        /// <param name="port">The port number on the LDAP host.</param>
        public LdapServer(string host, int port) :
            this()
        {
            m_serverIdentifier = new LdapDirectoryIdentifier(host, port);
        }

        /// <summary>
        /// Create a connection to the LDAP server at the given host and port.
        /// The caller can specify whether or not to use SSL and whether or not
        /// to do certificate verification.
        /// </summary>
        /// <param name="host">The FQDN or IP of the LDAP host.</param>
        /// <param name="port">The port number on the LDAP host.</param>
        /// <param name="useSsl">Whether or not to use SSL.</param>
        /// <param name="verifyCert">Whether or not to attempt to verify the server's certficate against
        /// the user/machine certificate store.</param>
        public LdapServer(string host, int port, bool useSsl, bool verifyCert) : this()
        {
            m_serverIdentifier = new LdapDirectoryIdentifier(host, port);
            m_useSsl = useSsl;
            m_verifyCert = verifyCert;
        }

        /// <summary>
        /// Create a connection to the LDAP server at the given host and port.
        /// Use of this constructor implies that the connection will be made with
        /// SSL enabled.  The server's SSL certificate will be verified against
        /// cert.
        /// </summary>
        /// <param name="host">The FQDN or IP of the LDAP host.</param>
        /// <param name="port">The port number of the LDAP host.</param>
        /// <param name="cert">A certificate to verify against the server's certificate.</param>
        public LdapServer(string host, int port, X509Certificate2 cert) : this()
        {
            m_serverIdentifier = new LdapDirectoryIdentifier(host, port);
            m_useSsl = true;
            m_verifyCert = true;
            m_cert = cert;
        }

        public void Connect(int timeout)
        {
            // Are we re-connecting?  If so, close the previous connection.
            if (m_conn != null)
            {
                this.Close();
            }

            m_conn = new LdapConnection(m_serverIdentifier);
            m_conn.Timeout = new System.TimeSpan(0,0,timeout);
            m_logger.DebugFormat("Timeout set to {0} seconds.", timeout);
            LdapSessionOptions opts = m_conn.SessionOptions;
            opts.ProtocolVersion = 3;
            opts.SecureSocketLayer = m_useSsl;
            if( m_useSsl )
                opts.VerifyServerCertificate = this.VerifyCert;
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
            m_logger.Debug("VerifyCert(...)");

            // Convert to X509Certificate2
            X509Certificate2 serverCert = new X509Certificate2(cert);

            // If we don't need to verify the cert, the verification succeeds
            if (!m_verifyCert)
            {
                m_logger.Debug("Server certificate accepted without verification.");
                return true;
            }

            // If the certificate is null, then we verify against the machine's/user's certificate store
            if (m_cert == null)
            {
                m_logger.Debug("Verifying server cert with Windows store.");
                // Use default policy
                X509ChainPolicy policy = new X509ChainPolicy();

                // Validation against the user's certificate store
                X509CertificateValidator validator = X509CertificateValidator.CreateChainTrustValidator(false, policy);
                try
                {
                    validator.Validate(serverCert);

                    // If we get here, validation succeeded.
                    m_logger.Debug("Server certificate verification succeeded.");
                    return true;
                }
                catch (SecurityTokenValidationException)
                {
                    m_logger.Debug("Server certificate validation failed.");
                    return false;
                }
            }
            else
            {
                m_logger.Debug("Validating server certificate with provided certificate.");

                // Verify against the provided cert by comparing the thumbprint
                bool result = m_cert.Thumbprint == serverCert.Thumbprint;
                if (result) m_logger.Debug("Server certificate validated.");
                else m_logger.Debug("Server certificate validation failed.");
                return result;
            }
        }

        /// <summary>
        /// Tries to bind to the server anonymously.  Throws LdapException if the
        /// bind fails.
        /// </summary>
        public void Bind()
        {
            if (m_conn == null)
                throw new LdapException("Bind attempted when server is not connected.");

            m_conn.AuthType = AuthType.Anonymous;
            m_conn.Credential = null;
            try
            {
                m_conn.Bind();
            }
            catch (LdapException e)
            {
                // TODO: probably log this exception
                throw e;
            }
            catch (InvalidOperationException e)
            {
                // This shouldn't happen, but log it and re-throw
                // TODO: log this exception
                throw e;
            }
        }

        /// <summary>
        /// Try to bind to the LDAP server with the given credentials.  This uses
        /// basic authentication.  Throws LdapException if the bind fails.
        /// </summary>
        /// <param name="creds">The credentials to use when binding.</param>
        public void Bind(NetworkCredential creds)
        {
            if (m_conn == null)
                throw new LdapException("Bind attempted when server is not connected.");

            m_conn.AuthType = AuthType.Basic;
            try
            {
                m_conn.Bind(creds);
            }
            catch (LdapException e)
            {
                // TODO: probably log this exception
                throw e;
            }
            catch (InvalidOperationException e)
            {
                // This shouldn't happen, but log it and re-throw
                // TODO: log this exception
                throw e;
            }
        }

        public void Close()
        {
            if (m_conn != null)
            {
                m_conn.Dispose();
                m_conn = null;
            }
        }

        /// <summary>
        /// Does a search in the subtree at searchBase, using the filter provided and 
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

        public void Dispose()
        {
            this.Close();
        }
    }
}
