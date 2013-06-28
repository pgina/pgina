/*
    Written by Florian Rohmer (2013)
     
    Distribued under the pGina license.
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
using System.Text.RegularExpressions;
using System.Security;
using System.Security.Cryptography;
using Microsoft.Win32;

using log4net;

namespace pGina.Plugin.SingleUserSwitcher
{
    class Session
    {
        private static ILog m_logger = LogManager.GetLogger("SingleUserSwitcher_Session");

        public string ldapgroup { get { return m_ldapgroup; } }
        protected string m_ldapgroup;

        public string username { get { return m_username; } }
        protected string m_username;

        public string password { get { return m_password; } }
        protected string m_password;

        public Session(string ldapgroup, string username, string password)
        {
            m_ldapgroup = ldapgroup; 
            m_username = username;
            m_password = password;
        }

        /// <summary>
        /// for plugin configuration window
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";
            str = string.Format("If member of LDAP group \"{0}\" ", m_ldapgroup);
            str += "open session \"" + username + "\"";
            return str;
        }

        /// <summary>
        /// returns the session in registry format (group\nusername\nencryptedpassword)
        /// </summary>
        /// <returns></returns>
        public string ToRegString()
        {
            return string.Format("{0}\n{1}\n{2}", m_ldapgroup, m_username, Encrypt(m_password));
        }

        /// <summary>
        /// encrypts password for registry storage
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private string Encrypt(string password)
        {
            byte[] valueBytes = UnicodeEncoding.Default.GetBytes(password);
            byte[] protectedBytes = ProtectedData.Protect(valueBytes, null, DataProtectionScope.LocalMachine);
            string base64 = Convert.ToBase64String(protectedBytes);
            return base64;
        }

        /// <summary>
        /// decrypts registry password
        /// </summary>
        /// <param name="base64"></param>
        /// <returns></returns>
        private static string Decrypt(string base64)
        {
            byte[] protectedBytes = Convert.FromBase64String(base64);
            byte[] valueBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.LocalMachine);
            return UnicodeEncoding.Default.GetString(valueBytes);
        }

        /// <summary>
        /// stores every session in registry
        /// </summary>
        /// <param name="sessions"></param>
        public static void SaveSessions(List<Session> sessions)
        {
            List<string> strList = new List<string>();
            foreach (Session sess in sessions)
            {
                strList.Add(sess.ToRegString());
            }
            Settings.Store.SessionList = strList.ToArray();
        }

        /// <summary>
        /// returns a list of the stored sessions
        /// </summary>
        /// <returns></returns>
        public static List<Session> GetSessions()
        {
            List<Session> sess = new List<Session>();
            string[] strSess = Settings.Store.SessionList;
            foreach (string str in strSess)
            {
                Session session = Session.FromRegString(str);
                if (session != null)
                    sess.Add(session);
                else
                    // Log error
                    m_logger.ErrorFormat("Unrecognized registry entry when loading session parameters, ignoring: {0}", str);
            }
            return sess;
        }

        /// <summary>
        /// converts registry sessions to Session objects
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Session FromRegString(string str)
        {
            string[] parts = Regex.Split(str, @"\n");
            if (parts.Length == 3)
            {
                string ldapgroup = parts[0];
                string username = parts[1];
                string pwd = Session.Decrypt(parts[2]);
                return new Session(ldapgroup, username, pwd);
            }
            return null;


        }
    }
}
