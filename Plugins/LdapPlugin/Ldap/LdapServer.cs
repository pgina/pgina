/*
	Copyright (c) 2018, pGina Team
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
using System.Reflection;
using System.DirectoryServices.Protocols;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

using pGina.Shared.Types;

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
        /// Whether or not to use SSL
        /// </summary>
        private bool m_useTls;

        /// <summary>
        /// Whether or not to verify the SSL certificate
        /// </summary>
        private bool m_verifyCert;

        /// <summary>
        /// Check the SSL certificate against a copy in local machine cert store
        /// </summary>
        private bool m_verifyCertMatch;

        /// <summary>
        /// The SSL certificate to verify against (if required)
        /// </summary>
        private X509Certificate2 m_cert;

        /// <summary>
        /// The SSL certificate to verify against (if required)
        /// </summary>
        private List<string> m_hostname;

        private Dictionary<string, string> pGinaSettings = new Dictionary<string, string>();

        /// <summary>
        /// The number of seconds to wait for a connection before giving up.
        /// </summary>
        public int Timeout { get; set; }

        public LdapServer()
        {
            m_conn = null;
            m_cert = null;
            Timeout = Settings.Store.LdapTimeout;
            m_useSsl = Settings.Store.UseSsl;
            m_useTls = Settings.Store.UseTls;
            m_verifyCert = Settings.Store.RequireCert;
            string certFile = Settings.Store.ServerCertFile;
            if (certFile.Equals("MATCH"))
            {
                certFile = "";
                m_verifyCertMatch = true;
            }
            if ((m_useSsl || m_useTls) && m_verifyCert)
            {
                if ( !string.IsNullOrEmpty(certFile) && File.Exists(certFile))
                {
                    m_logger.DebugFormat("Loading server certificate: {0}", certFile);
                    m_cert = new X509Certificate2(certFile);
                }
                else
                {
                    m_logger.DebugFormat("Certificate file not provided or not found, will validate against Windows store.", certFile);
                }
            }

            string[] hosts = Settings.Store.LdapHost;
            int port = Settings.Store.LdapPort;
            m_serverIdentifier = new LdapDirectoryIdentifier(hosts, port, false, false);
            m_hostname = hosts.ToList();

            m_logger.DebugFormat("Initializing LdapServer host(s): [{0}], port: {1}, useSSL = {2}, useTLS = {3}, verifyCert = {4}",
                string.Join(", ", hosts), port, m_useSsl, m_useTls, m_verifyCert);

            pGinaSettings = pGina.Shared.Settings.pGinaDynamicSettings.GetSettings(pGina.Shared.Settings.pGinaDynamicSettings.pGinaRoot, new string[] { "notify_pass" });

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
            m_conn.Timeout = new System.TimeSpan(0,0,Timeout);
            m_logger.DebugFormat("Timeout set to {0} seconds.", Timeout);
            m_conn.SessionOptions.ProtocolVersion = 3;
            if ((m_useSsl || m_useTls) && m_verifyCert)
            {
                m_conn.SessionOptions.VerifyServerCertificate = new VerifyServerCertificateCallback(VerifyCert);
            }
            else
            {
                m_conn.SessionOptions.VerifyServerCertificate = new VerifyServerCertificateCallback((conn, cert) => true);
            }
            if (m_useTls)
            {
                try
                {
                    m_conn.SessionOptions.StartTransportLayerSecurity(new DirectoryControlCollection());
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("Start TLS failed with {0}", e.Message);
                    m_conn = null;
                }
            }
            if (m_useSsl)
            {
                m_conn.SessionOptions.SecureSocketLayer = m_useSsl;
            }
        }

        /// <summary>
        /// Check that certificate CN or SubjectAltName matches LDAP server name
        /// </summary>
        /// <param name="conn">The LDAP connection.</param>
        /// <param name="cert">The server's certificate</param>
        /// <returns>true if verification succeeds, false otherwise.</returns>
        private bool VerifyCertName(LdapConnection conn, X509Certificate2 cert)
        {
            string certCN = "";
            List<string> name = new List<string>() { conn.SessionOptions.HostName };

            try
            {
                List<IPAddress> Ipaddr = new List<IPAddress>();
                IPHostEntry hostFQDN = Dns.GetHostEntry(conn.SessionOptions.HostName);
                Ipaddr = hostFQDN.AddressList.ToList();

                foreach (string host in m_hostname)
                {
                    hostFQDN = Dns.GetHostEntry(host.Trim());
                    if (hostFQDN.AddressList.ToList().Any(x => Ipaddr.Contains(x)))
                    {
                        name.Add(host.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                m_logger.InfoFormat("Error in VerifyCertName() during GetHostEntry:{0}", ex);
            }

            m_logger.InfoFormat("Verify DNS name against:{0}", string.Join(" ", name));

            string[] str = cert.SubjectName.Decode(X500DistinguishedNameFlags.DoNotUsePlusSign | X500DistinguishedNameFlags.DoNotUseQuotes | X500DistinguishedNameFlags.UseNewLines | X500DistinguishedNameFlags.UseUTF8Encoding).Trim('\r').Split('\n');
            for (int x = 0; x < str.Length; x++)
            {
                if (str[x].StartsWith("CN="))
                    certCN = str[x].Replace("CN=", "").Trim();
            }
            certCN = certCN.Replace(".", @"\.").Replace("*", ".*");
            if (name.Any(x => Regex.IsMatch(x, "^" + certCN)))
            {
                return true;
            }

            List<string> certSAN = new List<string>();
            foreach (X509Extension extension in cert.Extensions)
            {
                AsnEncodedData asndata = new AsnEncodedData(extension.Oid, extension.RawData);
                string[] adata = asndata.Format(true).Trim('\r').Split('\n');
                for (int x = 0; x < adata.Length; x++)
                {
                    if (adata[x].StartsWith("DNS-Name="))
                    {
                        string SANregex = adata[x].Replace("DNS-Name=", "").Trim();
                        SANregex = SANregex.Replace(".", @"\.").Replace("*", ".*");
                        certSAN.Add(SANregex);
                    }
                }
            }
            foreach (string csan in certSAN)
            {
                if (name.Any(x => Regex.IsMatch(x, "^" + csan)))
                {
                    return true;
                }
            }

            return false;
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
            m_logger.DebugFormat("Verifying certificate from host: {0}", conn.SessionOptions.HostName);
            
            // Convert to X509Certificate2
            X509Certificate2 serverCert = new X509Certificate2(cert);

            // If we don't need to verify the cert, the verification succeeds
            if (!m_verifyCert)
            {
                m_logger.Debug("Server certificate accepted without verification.");
                return true;
            }

            if (m_verifyCertMatch)
            {
                try
                {
                    X509Store certStore = new X509Store(StoreLocation.LocalMachine);
                    certStore.Open(OpenFlags.ReadOnly);
                    if (certStore.Certificates.Contains(cert))
                        return true;
                    else
                    {
                        m_logger.ErrorFormat("Can't find server cert in Windows store. Place it in Local Computer cert store.");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("Server certificate validation exception: {0}", e.Message);
                }

                return false;
            }

            // If the certificate is null, then we verify against the machine's/user's certificate store
            if (m_cert == null)
            {
                m_logger.Debug("Verifying server cert with Windows store.");

                try
                {
                    if (!serverCert.Verify())
                    {
                        m_logger.Debug("Server certificate verification failed.");
                        return false;
                    }

                    // Check that certificate name match server name
                    if (!VerifyCertName(conn, serverCert))
                    {
                        m_logger.Debug("Server certificate does not match hostname.");
                        return false;
                    }

                    // If we get here, validation succeeded.
                    m_logger.Debug("Server certificate verification succeeded.");
                    return true;
                }
                catch (SecurityTokenValidationException e)
                {
                    m_logger.ErrorFormat("Server certificate validation failed: {0}", e.Message);
                    return false;
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("Server certificate validation failed: {0}", e.Message);
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

            m_logger.DebugFormat("Attempting anonymous bind", m_conn.SessionOptions.HostName);

            m_conn.AuthType = AuthType.Anonymous;
            m_conn.Credential = null;

            try
            {
                m_conn.Bind();
                m_logger.DebugFormat("Successful bind to {0}", m_conn.SessionOptions.HostName);
            }
            catch (LdapException e)
            {
                m_logger.ErrorFormat("LdapException: {0} {1} {2}", e.ErrorCode, e.Message, e.ServerErrorMessage);
                throw e;
            }
            catch (InvalidOperationException e)
            {
                // This shouldn't happen, but log it and re-throw
                m_logger.ErrorFormat("InvalidOperationException: {0}", e.Message);
                throw e;
            }
            catch (Exception e)
            {
                // This shouldn't happen, but log it and re-throw
                m_logger.ErrorFormat("Bind Exception: {0}", e.Message);
                throw e;
            }
        }

        public void BindForSearch()
        {
            string searchDn = Settings.Store.SearchDN;
            string searchPw = Settings.Store.GetEncryptedSetting("SearchPW");

            if (string.IsNullOrEmpty(searchDn))
                // Bind anonymously
                this.Bind();
            else
                // Bind with credentials
                this.Bind(new NetworkCredential(searchDn, searchPw));
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

            m_logger.DebugFormat("Attempting bind as {0}", creds.UserName);

            m_conn.AuthType = AuthType.Basic;

            try
            {
                m_conn.Bind(creds);
                m_logger.DebugFormat("Successful bind to {0} as {1}", m_conn.SessionOptions.HostName, creds.UserName);
            }
            catch (LdapException e)
            {
                m_logger.ErrorFormat("LdapException: {0} {1} {2}", e.ErrorCode, e.Message, e.ServerErrorMessage);
                throw e;
            }
            catch (InvalidOperationException e)
            {
                // This shouldn't happen, but log it and re-throw
                m_logger.ErrorFormat("InvalidOperationException: {0}", e.Message);
                throw e;
            }
            catch (Exception e)
            {
                // This shouldn't happen, but log it and re-throw
                m_logger.ErrorFormat("Bind Exception: {0}", e.Message);
                throw e;
            }
        }

        public void Close()
        {
            if (m_conn != null)
            {
                m_logger.DebugFormat("Closing LDAP connection to {0}.", m_conn.SessionOptions.HostName);
                if (m_useTls)
                    m_conn.SessionOptions.StopTransportLayerSecurity();
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
            SearchRequest req = new SearchRequest(searchBase, filter, SearchScope.Subtree, null);
            SearchResponse resp = (SearchResponse)m_conn.SendRequest(req);

            if (resp.Entries.Count > 0)
            {
                return resp.Entries[0].DistinguishedName;
            }

            return null;
        }

        public bool MemberOfGroup(string user, string group)
        {
            string groupDn = Settings.Store.GroupDnPattern;
            string groupAttribute = Settings.Store.GroupMemberAttrib;

            if (string.IsNullOrEmpty(groupDn))
                throw new Exception("Can't resolve group DN, group DN pattern missing.");

            if (string.IsNullOrEmpty(groupAttribute))
                throw new Exception("Can't resolve group membership, group attribute missing.");

            groupDn = Regex.Replace(groupDn, @"\%g", group);

            string target = user;

            // If the group attribute is "uniqueMember" or "member" then the LDAP server
            // is using groupOfUniqueNames or groupOfNames object class.  The group
            // list uses full DNs instead of just uids, so we need to expand the
            // username to the full DN.
            if (groupAttribute.Equals("uniqueMember", StringComparison.CurrentCultureIgnoreCase) ||
                groupAttribute.Equals("member", StringComparison.CurrentCultureIgnoreCase))
            {
                // Try to generate the full DN for the user.
                m_logger.DebugFormat("Attempting to generate DN for user {0}", user);
                target = this.GetUserDN(user);
                if (target == null)
                {
                    m_logger.Error("Unable to generate DN for user, using username.");
                    target = user;
                }
            }

            string filter = string.Format("({0}={1})", groupAttribute, target);
            m_logger.DebugFormat("Searching for group membership, DN: {0}  Filter: {1}", groupDn, filter);
            try
            {
                SearchRequest req = new SearchRequest(groupDn, filter, SearchScope.Base, null);
                SearchResponse resp = (SearchResponse)m_conn.SendRequest(req);
                return resp.Entries.Count > 0;
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Error when checking for group membership: {0}", e.Message);
                return false;
            }
        }

        public void Dispose()
        {
            this.Close();
        }

        /// <summary>
        /// Attempt to authenticate the user by binding to the LDAP server.
        /// </summary>
        /// <returns></returns>
        public BooleanResult Authenticate(string uname, string password, SessionProperties properties)
        {
            // Check for empty password.  If configured to do so, we fail on
            // empty passwords.
            bool allowEmpty = Settings.Store.AllowEmptyPasswords;
            if (!allowEmpty && string.IsNullOrEmpty(password))
            {
                m_logger.Info("Authentication failed due to empty password.");
                return new BooleanResult { Success = false, Message = "Authentication failed due to empty password." };
            }

            // Get the user's DN
            string userDN = "";
            try
            {
                userDN = GetUserDN(uname);
            }
            catch (Exception ex)
            {
                return new BooleanResult { Success = false, Message = ex.Message };
            }

            // If we've got a userDN, attempt to authenticate the user
            if (userDN != null)
            {
                // Attempt to bind with the user's LDAP credentials
                m_logger.DebugFormat("Attempting to bind with DN {0}", userDN);
                NetworkCredential ldapCredential = new NetworkCredential(userDN, password);
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

                try
                {
                    this.Bind(ldapCredential);
                }
                catch (LdapException e)
                {
                    // 49 is invalid credentials
                    if (e.ErrorCode == 49)
                    {
                        if (PWDexpired(uname, password).Success)
                        {
                            m_logger.InfoFormat("Password expired");
                            userInfo.PasswordEXP = true;
                            properties.AddTrackedSingle<UserInformation>(userInfo);
                            return new BooleanResult { Message = "Password expired", Success = true };
                        }

                        m_logger.ErrorFormat("LDAP bind failed: invalid credentials.");
                        return new BooleanResult { Success = false, Message = "Authentication via LDAP failed. Invalid credentials." };
                    }

                    // Let caller handle other kinds of exceptions
                    throw;
                }
                catch (Exception e)
                {
                    // IBM LDAP doesnt return error 49 so we analyze the exeption string
                    if (e.ToString().Contains("pGina.Plugin.Ldap.LdapServer.Bind"))
                    {
                        m_logger.ErrorFormat("The user name or password is incorrect: {0}", e.Message);
                        return new BooleanResult { Success = false, Message = "The user name or password is incorrect" };
                    }

                    m_logger.ErrorFormat("LDAP plugin failed {0}",e.Message);
                    return new BooleanResult { Success = false, Message = String.Format("LDAP plugin failed\n{0}",e.Message) };
                }

                // If we get here, the authentication was successful, we're done!
                m_logger.DebugFormat("LDAP DN {0} successfully bound to server, return success", ldapCredential.UserName);

                BooleanResultEx pwd = PWDexpired(uname, password);
                if (pwd.Success) //samba ldap may not throw exception 49
                {
                    m_logger.InfoFormat("Password expired");
                    userInfo.PasswordEXP = true;
                    properties.AddTrackedSingle<UserInformation>(userInfo);
                    return new BooleanResult { Message = "Password expired", Success = true };
                }
                else
                {
                    userInfo.PasswordEXPcntr = new TimeSpan(pwd.Int64);
                    properties.AddTrackedSingle<UserInformation>(userInfo);
                }

                try
                {
                    string[] AttribConv = Settings.Store.AttribConv;
                    Dictionary<string, string> Convert_attribs = new Dictionary<string, string>();
                    foreach (string str in AttribConv)
                    {
                        if (Regex.IsMatch(str, @"\w\t\w"))
                        {
                            // Convert_attribs.add("Email", "mail")
                            Convert_attribs.Add(str.Substring(0, str.IndexOf('\t')).Trim(), str.Substring(str.IndexOf('\t')).Trim());
                        }
                    }
                    if (Convert_attribs.Count > 0)
                    {
                        // search all values at once
                        Dictionary<string, List<string>> search = GetUserAttribValue(userDN, "(objectClass=*)", SearchScope.Subtree, Convert_attribs.Values.ToArray());
                        if (search.Count > 0)
                        {
                            foreach (KeyValuePair<string, List<string>> search_p in search)
                            {
                                foreach (KeyValuePair<string, string> Convert_attribs_p in Convert_attribs)
                                {
                                    // Convert_attribs_p.add("Email", "mail")
                                    // search_p.add("mail", "user@test.local")
                                    // if Convert_attribs_p.value == search_p.key (if mail == mail)
                                    if (Convert_attribs_p.Value.Equals(search_p.Key, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        // loop through all props of UserInformation
                                        foreach (PropertyInfo prop in userInfo.GetType().GetProperties())
                                        {
                                            // if prop.name == Convert_attribs_p.key (if Email == Email)
                                            if (prop.Name.Equals(Convert_attribs_p.Key, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                // set this value (userinfo.Email = "user@test.local")
                                                try
                                                {
                                                    object o = Convert.ChangeType(search_p.Value.First(), prop.PropertyType);
                                                    prop.SetValue(userInfo, o, null);
                                                    m_logger.DebugFormat("convert attrib:[{0}] to [{1}] value:[{2}]", search_p.Key, Convert_attribs_p.Key, search_p.Value.First());
                                                }
                                                catch (Exception ex)
                                                {
                                                    m_logger.ErrorFormat("can't convert attrib:[{0}] to [{1}] Error:[{2}]", search_p.Key, Convert_attribs_p.Key, ex.Message);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("can't convert ldap value", e.Message);
                }

                return new BooleanResult { Success = true };
            } // end if(userDN != null)
            else
            {
                m_logger.ErrorFormat("Unable to determine DN for: {0}", uname);
                return new BooleanResult { Success = false, Message = "Unable to determine the user's LDAP DN for authentication." };
            }
        }

        internal BooleanResultEx PWDexpired(string uname, string password)
        {
            //m_logger.InfoFormat("PWDexpired");

            string userDN = GetUserDN(uname);
            if (userDN == null)
            {
                return new BooleanResultEx { Success = false };
            }
            //m_logger.InfoFormat("userDN={0}", userDN);

            List<string> ntps = m_serverIdentifier.Servers.ToList();
            ntps.AddRange(pGinaSettings["ntpservers"].Split('\0').ToList());
            m_logger.InfoFormat("ntplist:{0}", String.Join(" ", ntps));

            Dictionary<string, List<string>> search = GetUserAttribValue(userDN, "(objectClass=*)", SearchScope.Base, new string[] { "shadowMax", "sambaPwdMustChange", "userAccountControl", "msDS-User-Account-Control-Computed", "msDS-UserPasswordExpiryTimeComputed", "pwdLastSet", "memberOf", "msDS-PSOApplied" });
            #region samba
            if (search.ContainsKey("shadowmax"))
            {
                //m_logger.InfoFormat("shadowmax found={0}", search["shadowmax"].First());
                int shadowmax = -1;
                try
                {
                    shadowmax = Convert.ToInt32(search["shadowmax"].First());
                }
                catch (Exception e)
                {
                    m_logger.FatalFormat("Unable to convert \"{0}\" return from GetUserAttribValue to int {1}", "shadowmax", e.Message);
                }

                if (shadowmax == -1 /*samba DONT_EXPIRE_PASSWD*/)
                {
                    return new BooleanResultEx { Success = false };
                }

                try
                {
                    shadowmax = Convert.ToInt32(new TimeSpan(shadowmax, 0, 0, 0).TotalSeconds);
                }
                catch (Exception e)
                {
                    m_logger.FatalFormat("Unable to convert \"{0}\" to seconds {1}", "shadowmax", e.Message);
                    return new BooleanResultEx { Success = false };
                }
                //m_logger.InfoFormat("shadowmax={0}", shadowmax);

                if (search.ContainsKey("sambapwdmustchange"))
                {
                    //m_logger.InfoFormat("sambapwdmustchange found");
                    int sambapwdmustchange = 0;
                    try
                    {
                        sambapwdmustchange = Convert.ToInt32(search["sambapwdmustchange"].First());
                    }
                    catch (Exception e)
                    {
                        m_logger.FatalFormat("Unable to convert \"{0}\" return from GetUserAttribValue to int {1}", "sambapwdmustchange", e.Message);
                    }

                    if (sambapwdmustchange == 0)
                    {
                        return new BooleanResultEx { Success = false };
                    }
                    //m_logger.InfoFormat("sambapwdmustchange={0}", sambapwdmustchange);

                    int DateNowSec = 0;
                    DateTime NTPtime = DateTime.MaxValue;
                    try
                    {
                        DateTime time = Abstractions.Windows.Networking.GetNetworkTime(ntps.ToArray());
                        //m_logger.InfoFormat("NTP {0}", time);
                        if (time == DateTime.MinValue)
                        {
                            m_logger.InfoFormat("can't get time from {0}", String.Join(" ", ntps));
                            NTPtime = DateTime.MaxValue;
                        }
                        else
                        {
                            NTPtime = time;
                            DateNowSec = Convert.ToInt32((NTPtime - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
                        }
                    }
                    catch (Exception e)
                    {
                        m_logger.FatalFormat("Unable to convert NTP time to ticks {0}", e);
                    }

                    if (NTPtime != DateTime.MaxValue)
                    {
                        m_logger.InfoFormat("{0} > {1}", DateNowSec, sambapwdmustchange);
                        if (DateNowSec + 86400 /*one day in future*/ > sambapwdmustchange)
                        {
                            m_logger.InfoFormat("pwd expired");
                            return new BooleanResultEx { Success = true, Message = "Password expired" };
                        }

                        TimeSpan PWDlasts = new TimeSpan((long)(sambapwdmustchange - DateNowSec) * (long)10000000);
                        if (PWDlasts < new TimeSpan(5, 0, 0, 0))
                        {
                            m_logger.InfoFormat("password will expire in less than 5 days, to be exact:{0:c}", PWDlasts);
                            return new BooleanResultEx { Success = false, Int64 = PWDlasts.Ticks };
                        }
                    }
                }
            }
            #endregion
            #region AD
            if (search.ContainsKey("msds-user-account-control-computed"))
            {
                //m_logger.InfoFormat("msds-user-account-control-computed found={0}", search["msds-user-account-control-computed"].First());
                int msdsuseraccountcontrolcomputed = -1;
                try
                {
                    msdsuseraccountcontrolcomputed = Convert.ToInt32(search["msds-user-account-control-computed"].First());
                }
                catch (Exception e)
                {
                    m_logger.FatalFormat("Unable to convert \"{0}\" return from GetUserAttribValue to int {1}", "msds-user-account-control-computed", e.Message);
                }

                if (((int)Abstractions.WindowsApi.pInvokes.structenums.UserFlags.UF_DONT_EXPIRE_PASSWD & msdsuseraccountcontrolcomputed) == (int)Abstractions.WindowsApi.pInvokes.structenums.UserFlags.UF_DONT_EXPIRE_PASSWD || ((int)Abstractions.WindowsApi.pInvokes.structenums.UserFlags.UF_PASSWD_CANT_CHANGE & msdsuseraccountcontrolcomputed) == (int)Abstractions.WindowsApi.pInvokes.structenums.UserFlags.UF_PASSWD_CANT_CHANGE)
                {
                    m_logger.InfoFormat("userAccountControl contain UF_DONT_EXPIRE_PASSWD and or UF_PASSWD_CANT_CHANGE");
                    return new BooleanResultEx { Success = false };
                }

                if (((int)Abstractions.WindowsApi.pInvokes.structenums.UserFlags.UF_PASSWORD_EXPIRED & msdsuseraccountcontrolcomputed) == (int)Abstractions.WindowsApi.pInvokes.structenums.UserFlags.UF_PASSWORD_EXPIRED)
                {
                    m_logger.InfoFormat("userAccountControl contain UF_PASSWORD_EXPIRED");
                    return new BooleanResultEx { Success = true, Message = "Password expired" };
                }

                DateTime msdsuserpasswordexpirytimecomputed = DateTime.MaxValue;
                if (search.ContainsKey("msds-userpasswordexpirytimecomputed"))
                {
                    //m_logger.InfoFormat("msds-userpasswordexpirytimecomputed={0}", search["msds-userpasswordexpirytimecomputed"].First());
                    try
                    {
                        msdsuserpasswordexpirytimecomputed = DateTime.FromFileTimeUtc(Convert.ToInt64(search["msds-userpasswordexpirytimecomputed"].First()));
                    }
                    catch (Exception e)
                    {
                        m_logger.FatalFormat("Unable to convert \"{0}\" return from GetUserAttribValue to int {1}", "msds-userpasswordexpirytimecomputed", e.Message);
                        return new BooleanResultEx { Success = false };
                    }
                }
                if (msdsuserpasswordexpirytimecomputed == DateTime.MaxValue)
                {
                    return new BooleanResultEx { Success = false };
                }

                DateTime NTPtime = DateTime.MaxValue;
                try
                {
                    NTPtime = Abstractions.Windows.Networking.GetNetworkTime(ntps.ToArray());
                    //m_logger.InfoFormat("NTP {0:yyyy.MM.dd.HH:mm:ss}", NTPtime);
                    if (NTPtime == DateTime.MinValue)
                    {
                        m_logger.InfoFormat("can't get time from {0}", String.Join(" ", ntps));
                        NTPtime = DateTime.MaxValue;
                    }
                }
                catch (Exception e)
                {
                    m_logger.FatalFormat("Unable to convert NTP time to ticks {0}", e);
                }
                if (NTPtime != DateTime.MaxValue)
                {
                    m_logger.InfoFormat("{0:yyyy.MM.dd.HH:mm:ss}-{1:yyyy.MM.dd.HH:mm:ss}", msdsuserpasswordexpirytimecomputed, NTPtime);
                    TimeSpan PWDlasts = msdsuserpasswordexpirytimecomputed - NTPtime;
                    //m_logger.InfoFormat("PWDlasts:{0:c}", PWDlasts);
                    if (PWDlasts < new TimeSpan(1, 0, 0, 0))
                    {
                        return new BooleanResultEx { Success = true, Message = "Password expired" };
                    }
                    if (PWDlasts < new TimeSpan(5, 0, 0, 0))
                    {
                        m_logger.InfoFormat("password will expire in less than 5 days, to be exact:{0:c}", PWDlasts);
                        return new BooleanResultEx { Success = false, Int64 = PWDlasts.Ticks };
                    }
                }

                return new BooleanResultEx { Success = false };
            }
            #endregion

            return new BooleanResultEx { Success = false };
        }

        public string GetUserDN(string uname)
        {
            bool doSearch = Settings.Store.DoSearch;
            if (!doSearch)
            {
                return CreateUserDN(uname);
            }
            else
            {
                return FindUserDN(uname);
            }
        }

        /// <summary>
        /// Will search an attribute and return the corresponding values
        /// <para>The DN where the search will start at</para>
        /// <para>string array of attributes to search at</para>
        /// <para>Searchscope</para>
        /// <para>Filter</para>
        /// return key ToLower()
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<string>> GetUserAttribValue(string path, string filter, SearchScope scope, string[] Attrib)
        {
            Dictionary<string, List<string>> ret = new Dictionary<string, List<string>>();

            try
            {
                SearchRequest req = new SearchRequest(path, filter, scope, Attrib);
                SearchResponse resp = (SearchResponse)m_conn.SendRequest(req);

                foreach (SearchResultEntry entry in resp.Entries)
                {
                    if (Attrib.All(element => element.Equals("dn", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        ret.Add("dn",new List<string>(new string[] {entry.DistinguishedName}));
                    }
                    foreach (String name in entry.Attributes.AttributeNames)
                    {
                        List<string> values = new List<string>();
                        for (int x = 0; x < entry.Attributes[name].Count; x++)
                        {
                            object res = entry.Attributes[name][x];
                            if (res == null)
                            {
                                break;
                            }

                            string str = "";
                            if (res.GetType() == typeof(byte[]))
                            {
                                foreach (byte b in (byte[])res)
                                {
                                    str += String.Format("{0:X2}", b);
                                }
                            }
                            else
                            {
                                str = res.ToString();
                            }

                            if (!String.IsNullOrEmpty(str))
                            {
                                values.Add(str);
                            }
                        }
                        if (values.Count > 0)
                        {
                            ret.Add(name.ToLower(),values);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                m_logger.FatalFormat("GetUserAttribValue Error:{0}",e.Message);
            }

            return ret;
        }

        /// <summary>
        /// Attempts to find the DN for the user by searching a set of LDAP trees.
        /// The base DN for each of the trees is retrieved from Settings.Store.SearchContexts.
        /// The search filter is taken from Settings.Store.SearchFilter.  If all
        /// searches fail, this method returns null.
        /// </summary>
        /// <returns>The DN of the first object found, or null if searches fail.</returns>
        private string FindUserDN(string uname)
        {
            // Attempt to bind in order to do the search
            this.BindForSearch();

            string filter = CreateSearchFilter(uname);

            m_logger.DebugFormat("Searching for DN using filter {0}", filter);
            string[] contexts = Settings.Store.SearchContexts;
            foreach (string context in contexts)
            {
                m_logger.DebugFormat("Searching context {0}", context);
                string dn = null;
                try
                {
                    dn = this.FindFirstDN(context, filter);
                }
                catch (DirectoryOperationException e)
                {
                    m_logger.ErrorFormat("DirectoryOperationException: {0}", e.Message);
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("FindUserDN failed: {0}", e.Message);
                    return null;
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
        private string CreateUserDN(string uname)
        {
            string result = Settings.Store.DnPattern;

            // Replace the username
            result = Regex.Replace(result, @"\%u", uname);

            return result;
        }

        /// <summary>
        /// This generates the search filter to be used when searching for the DN
        /// </summary>
        /// <returns>A search filter.</returns>
        private string CreateSearchFilter(string uname)
        {
            string result = Settings.Store.SearchFilter;

            // Replace the username
            result = Regex.Replace(result, @"\%u", uname);

            return result;
        }

        public bool SetUserAttribute(string uname, string attribute, string value)
        {
            string userDN = this.GetUserDN(uname);

            try
            {
                DirectoryAttributeModification mod = new DirectoryAttributeModification
                {
                    Name = attribute,
                    Operation = DirectoryAttributeOperation.Replace
                };
                mod.Add(value);
                ModifyRequest req = new ModifyRequest(userDN);
                req.Modifications.Add(mod);
                m_conn.SendRequest(req);
            }
            catch (Exception e)
            {
                m_logger.FatalFormat("can't add attribute:{0} because of error:{1}", attribute, e.Message);
                return false;
            }

            if (attribute.ToLower().Equals("sambapwdlastset"))
            {
                Dictionary<string, List<string>> SearchResult = GetUserAttribValue(userDN, "(objectClass=*)", SearchScope.Subtree, new string[] { "shadowMax", "sambaPwdMustChange" });

                if (SearchResult.ContainsKey("shadowmax") && SearchResult.ContainsKey("sambapwdmustchange"))
                {
                    int shadowMax = 0;

                    try
                    {
                        shadowMax = Convert.ToInt32(SearchResult["shadowmax"].First());
                    }
                    catch (Exception e)
                    {
                        m_logger.FatalFormat("SetUserAttribute: Unable to convert return from GetUserAttribValue to int {0}", e.Message);
                        return false;
                    }

                    if (shadowMax > 0)
                    {
                        TimeMethod time = TimeMethod.methods[Methods.Timestamps];
                        string t = time.time(new TimeSpan(shadowMax, 0, 0, 0));
                        if (!t.Equals("0"))
                            if (!SetUserAttribute(uname, "sambaPwdMustChange", t))
                                return false;
                    }
                }
            }
            return true;
        }
    }
}
