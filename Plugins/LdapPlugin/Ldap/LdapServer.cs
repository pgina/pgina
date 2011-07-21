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
            Connect();
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
            Connect();
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
            Connect();
        }

        private void Connect()
        {
            // Are we re-connecting?  If so, close the previous connection.
            if (m_conn != null)
            {
                this.Close();
            }

            m_conn = new LdapConnection(m_serverIdentifier);
            LdapSessionOptions opts = m_conn.SessionOptions;
            opts.ProtocolVersion = 3;
            opts.SecureSocketLayer = m_useSsl;
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
            if (!m_useSsl) return false;

            // If we don't need to verify the cert, the verification succeeds
            if (!m_verifyCert) return true;

            // If the certificate is null, then we verify against the machine's/user's certificate store
            if (m_cert == null)
            {
                // Use default policy
                X509ChainPolicy policy = new X509ChainPolicy();

                // Validation against the user's certificate store
                X509CertificateValidator validator = X509CertificateValidator.CreateChainTrustValidator(false, policy);
                try
                {
                    validator.Validate(m_cert);
                    return true;  // If we get here, validation succeeded
                }
                catch (SecurityTokenValidationException)
                {
                    return false;
                }
            }
            else
            {
                // Verify against the provided cert by comparing the thumbprints
                X509Certificate2 serverCert = new X509Certificate2(cert);

                return m_cert.Thumbprint == serverCert.Thumbprint;
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

        public void Dispose()
        {
            this.Close();
        }
    }
}
