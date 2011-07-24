using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.DirectoryServices.Protocols;
using System.Text.RegularExpressions;
using System.IO;

using log4net;

using pGina.Interfaces;

namespace pGina.Plugin.Ldap
{
    class LdapAuthenticator
    {
        private ILog m_logger = LogManager.GetLogger("LdapAuthenticator");
        private NetworkCredential m_creds;

        public LdapAuthenticator(NetworkCredential creds)
        {
            m_creds = creds;
        }

        public AuthenticationResult Authenticate()
        {
            // Generate username (if we're not doing a search for it)
            string userDN = null;
            if (!LdapPlugin.Settings.DoSearch)
            {
                userDN = CreateUserDN();
            }

            LdapServer serv = null;
            try
            {
                // Try each host
                foreach (string host in LdapPlugin.Settings.LdapHost)
                {
                    m_logger.DebugFormat("Attempting to connect to {0}:{1}", host, LdapPlugin.Settings.LdapPort);

                    // Create the server object
                    if (LdapPlugin.Settings.UseSsl)
                    {
                        if (LdapPlugin.Settings.RequireCert)
                        {
                            if (LdapPlugin.Settings.ServerCertFile.Length > 0 &&
                                File.Exists(LdapPlugin.Settings.ServerCertFile) )
                            {
                                m_logger.DebugFormat("Using SSL and validating with provided server certificate");
                                m_logger.DebugFormat("Loading server certificate: {0}", LdapPlugin.Settings.ServerCertFile);
                                X509Certificate2 cert = new X509Certificate2(LdapPlugin.Settings.ServerCertFile);
                                serv = new LdapServer(host, LdapPlugin.Settings.LdapPort, cert);
                            }
                            else
                            {
                                m_logger.DebugFormat("Using SSL and validating with Windows certificate store");
                                serv = new LdapServer(host, LdapPlugin.Settings.LdapPort, true, true);
                            }
                        }
                        else
                        {
                            m_logger.DebugFormat("Using SSL without server certificate validation.");
                            serv = new LdapServer(host, LdapPlugin.Settings.LdapPort, true, false);
                        }
                    } // end if UseSsl
                    else
                    {
                        // We're not using SSL
                        serv = new LdapServer(host, LdapPlugin.Settings.LdapPort);
                    }

                    // Connect.  Note that this always succeeds whether or not the server is 
                    // actually available.  It doesn't actually talk to the server at all.  
                    // The timeout only takes effect when binding.
                    serv.Connect(LdapPlugin.Settings.LdapTimeout);

                    // If we're searching, attempt to bind with the search credentials, or anonymously
                    if (LdapPlugin.Settings.DoSearch)
                    {
                        // Set this to null (should be null anyway) because we are going to search
                        // for it.
                        userDN = null;
                        try
                        {
                            // Attempt to bind in order to do the search
                            if (LdapPlugin.Settings.SearchDN.Length > 0)
                            {
                                NetworkCredential creds = new NetworkCredential(LdapPlugin.Settings.SearchDN, LdapPlugin.Settings.SearchPW);
                                m_logger.DebugFormat("Attempting to bind with DN: {0} for search", creds.UserName);
                                serv.Bind(creds);
                            }
                            else
                            {
                                m_logger.DebugFormat("Attempting to bind anonymously for search.");
                                serv.Bind();
                            }

                            // If we get here, a bind was successful, so we can search for the user's DN
                            userDN = FindUserDN(serv);

                        }
                        catch (LdapException e)
                        {
                            if (e.ErrorCode == 81)
                            {
                                m_logger.ErrorFormat("Server {0} unavailable, moving on.", host);
                            }
                            else if (e.ErrorCode == 49)
                            {
                                m_logger.ErrorFormat("Bind failed: invalid credentials.");
                            }
                            else
                            {
                                m_logger.ErrorFormat("Exception ({0}) when binding for search: {1}", e.ErrorCode, e);
                            }
                        }
                    }

                    // If we've got a userDN, attempt to authenticate the user
                    if (userDN != null)
                    {
                        try
                        {
                            // Attempt to bind with the user's LDAP credentials
                            m_logger.DebugFormat("Attempting to bind with DN {0}", userDN);
                            NetworkCredential ldapCredential = new NetworkCredential(userDN, m_creds.Password);
                            serv.Bind(ldapCredential);

                            // If we get here, the authentication was successful, we're done!
                            m_logger.DebugFormat("LDAP DN {0} successfully bound to server, return success", ldapCredential.UserName);
                            return new AuthenticationResult { Success = true };
                        }
                        catch (LdapException e)
                        {
                            if (e.ErrorCode == 81)
                            {
                                m_logger.ErrorFormat("Server {0} unavailable, moving on.", host);
                            }
                            else if (e.ErrorCode == 49)
                            {
                                m_logger.ErrorFormat("Bind failed for LDAP DN {0}: invalid credentials.", userDN);
                            }
                            else
                            {
                                m_logger.ErrorFormat("Exception ({0}) when binding for authentication: {1}", e.ErrorCode, e.Message);
                            }
                        }

                    } // end if(userDN != null)

                    if (serv != null)
                    {
                        m_logger.DebugFormat("Closing connection to server {0}", host);
                        serv.Dispose();
                    }

                } // end loop over hosts
            }
            catch (Exception e)
            {
                if (e is LdapException)
                {
                    m_logger.ErrorFormat("LdapException ({0}): {1}", ((LdapException)e).ErrorCode, e);
                }
                else
                {
                    m_logger.DebugFormat("Exception: {0}", e);
                }
            }

            return new AuthenticationResult{ Success = false };
        }

        /// <summary>
        /// Attempts to find the DN for the user by searching a set of LDAP trees.
        /// The base DN for each of the trees is retrieved from LdapPlugin.Settings.SearchContexts.
        /// The search filter is taken from LdapPlugin.Settings.SearchFilter.  If all
        /// searches fail, this method returns null.
        /// </summary>
        /// <param name="serv">The LdapServer to use when performing the search.</param>
        /// <returns>The DN of the first object found, or null if searches fail.</returns>
        private string FindUserDN(LdapServer serv)
        {
            string filter = CreateSearchFilter();

            m_logger.DebugFormat("Searching for DN using filter {0}", filter);
            foreach( string context in LdapPlugin.Settings.SearchContexts )
            {
                m_logger.DebugFormat("Searching context {0}", context);
                string dn = null;
                try
                {
                    dn = serv.FindFirstDN(context, filter);
                }
                catch (DirectoryOperationException e)
                {
                    m_logger.ErrorFormat("DirectoryOperationException: {0}", e.Message);
                }
                if (dn != null)
                {
                    m_logger.DebugFormat("Found DN: {0}", dn);
                    return dn;
                }
            }

            m_logger.DebugFormat("No DN found in any of the contexts.");
            return null;
        }

        /// <summary>
        /// This generates the DN for the user assuming that a pattern has
        /// been provided.  This assumes that LdapPlugin.Settings.DnPattern has
        /// a valid DN pattern.
        /// </summary>
        /// <returns>A DN that can be used for binding with LDAP server.</returns>
        private string CreateUserDN()
        {
            string result = LdapPlugin.Settings.DnPattern;

            // Replace the username
            result = Regex.Replace(result, @"\%u", m_creds.UserName);

            // TODO: Replace other things...

            return result;
        }

        /// <summary>
        /// This generates the search filter to be used when searching for the DN
        /// </summary>
        /// <returns>A search filter.</returns>
        private string CreateSearchFilter()
        {
            string result = LdapPlugin.Settings.SearchFilter;

            // Replace the username
            result = Regex.Replace(result, @"\%u", m_creds.UserName);

            // TODO: Replace other things...

            return result;
        }

    }
}
