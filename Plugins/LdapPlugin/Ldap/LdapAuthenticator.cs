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
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.DirectoryServices.Protocols;
using System.Text.RegularExpressions;
using System.IO;

using log4net;

using pGina.Shared.Types;
using pGina.Shared.Interfaces;
using pGina.Shared.Settings;

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

        public BooleanResult Authenticate()
        {
            // Generate username (if we're not doing a search for it)
            string userDN = null;
            bool doSearch = Settings.Store.DoSearch;
            if ( ! doSearch )
            {
                userDN = CreateUserDN();
            }

            X509Certificate2 serverCert = null;
            bool useSsl = Settings.Store.UseSsl;
            bool requireCert = Settings.Store.RequireCert;
            string certFile = Settings.Store.ServerCertFile;
            if( useSsl && requireCert && certFile.Length > 0 ) 
            {
                if (File.Exists(certFile))
                {
                    m_logger.DebugFormat("Loading server certificate: {0}", certFile);
                    serverCert = new X509Certificate2(certFile);
                }
                else
                {
                    m_logger.ErrorFormat("Certificate file {0} not found, giving up.", certFile);
                    return new BooleanResult{ Success = false, Message = "Server certificate not found" };
                }
            }

            string[] hosts = Settings.Store.LdapHost;
            int port = Settings.Store.LdapPort;
            using (LdapServer serv = new LdapServer(hosts, port, useSsl, requireCert, serverCert))
            {
                try
                {
                    // Connect.  Note that this always succeeds whether or not the server is 
                    // actually available.  It not clear to me whether this actually talks to the server at all.  
                    // The timeout only seems to take effect when binding.
                    int timeout = Settings.Store.LdapTimeout;
                    serv.Connect(timeout);

                    // If we're searching, attempt to bind with the search credentials, or anonymously
                    if (doSearch)
                    {
                        // Set this to null (should be null anyway) because we are going to search
                        // for it.
                        userDN = null;
                        try
                        {
                            // Attempt to bind in order to do the search
                            string searchDN = Settings.Store.SearchDN;
                            string searchPW = Settings.Store.GetEncryptedSetting("SearchPW", null);
                            if (searchDN.Length > 0)
                            {
                                NetworkCredential creds = new NetworkCredential(searchDN, searchPW);
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
                                m_logger.ErrorFormat("Server unavailable", e.Message);
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
                            return new BooleanResult { Success = true };
                        }
                        catch (LdapException e)
                        {
                            if (e.ErrorCode == 81)
                            {
                                m_logger.ErrorFormat("Server unavailable");
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
            } // end using

            return new BooleanResult{ Success = false };
        }

        /// <summary>
        /// Attempts to find the DN for the user by searching a set of LDAP trees.
        /// The base DN for each of the trees is retrieved from Settings.Store.SearchContexts.
        /// The search filter is taken from Settings.Store.SearchFilter.  If all
        /// searches fail, this method returns null.
        /// </summary>
        /// <param name="serv">The LdapServer to use when performing the search.</param>
        /// <returns>The DN of the first object found, or null if searches fail.</returns>
        private string FindUserDN(LdapServer serv)
        {
            string filter = CreateSearchFilter();

            m_logger.DebugFormat("Searching for DN using filter {0}", filter);
            string[] contexts = Settings.Store.SearchContexts;
            foreach( string context in contexts )
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
        /// been provided.  This assumes that Settings.Store.DnPattern has
        /// a valid DN pattern.
        /// </summary>
        /// <returns>A DN that can be used for binding with LDAP server.</returns>
        private string CreateUserDN()
        {
            string result = Settings.Store.DnPattern;

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
            string result = Settings.Store.SearchFilter;

            // Replace the username
            result = Regex.Replace(result, @"\%u", m_creds.UserName);

            // TODO: Replace other things...

            return result;
        }

    }
}
