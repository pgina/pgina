/*
	Written by Evan Horne.
 
    Distributed under the pGina License.
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
using System.Diagnostics;
using System.Timers;

using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.IO;

using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using pGina.Shared.Settings;



namespace pGina.Plugin.Email
{
    public class EmailAuthPlugin : IPluginConfiguration, IPluginAuthentication
    {
        private ILog m_logger = LogManager.GetLogger("EmailAuthPlugin");
        public static Guid SimpleUuid = new Guid("{EC3221A6-621F-44CE-B77B-E074298D6B4E}");
        private string m_defaultDescription = "A plugin that authenticates against a POP or IMAP server.";
        private dynamic m_settings = null;

        public EmailAuthPlugin()
        {
            using(Process me = Process.GetCurrentProcess())
            {
                m_settings = new pGinaDynamicSettings(SimpleUuid);
                m_settings.SetDefault("ShowDescription", true);
                m_settings.SetDefault("Description", m_defaultDescription);
                
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }        

        public string Name
        {
            get { return "Email Authentication"; }
        }

        public string Description
        {
            get { return m_settings.Description; }
        }

        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public Guid Uuid
        {
            get { return SimpleUuid; }
        }
        
        BooleanResult IPluginAuthentication.AuthenticateUser(SessionProperties properties)
        {
            try
            {
                m_logger.DebugFormat("AuthenticateUser({0})", properties.Id.ToString());

                // Get user info, append domain if needed
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
                bool appendDomain = (bool) Settings.Store.AppendDomain;
                string username;
                if ((bool)Settings.Store.AppendDomain)
                    username = string.Format("{0}@{1}", userInfo.Username, (string)Settings.Store.Domain);
                else
                    username = userInfo.Username;

                // Place credentials into a NetworkCredentials object
                NetworkCredential creds = new NetworkCredential(username, userInfo.Password);
                
                string server = Settings.Store.Server;
                int port = Convert.ToInt32((string)Settings.Store.Port);
                bool useSsl = Settings.Store.UseSsl;
                string protocol = Settings.Store.Protocol;

                //Connect to server
                Stream stream = getNetworkStream(server, port, useSsl);
                bool authenticated;

                m_logger.DebugFormat("Have network stream...");

                //Authenticate based on protocol
                if (protocol == "POP3")
                    authenticated = authPop3(stream, creds);
                else
                    authenticated = authImap(stream, creds);

                if (authenticated)
                    return new BooleanResult() { Success = true };
                return new BooleanResult() { Success = false, Message = "Invalid username/password." };
            }
            catch (FormatException e)
            {   //Likely thrown if the port number can not be converted to an integer
                m_logger.ErrorFormat("Port number is not valid. Format exception: {0}", e);
                return new BooleanResult() { Success = false, Message = "Port number is not valid." };
            }
            catch (EMailAuthException e)
            {
                if (e.InnerException != null)
                    m_logger.ErrorFormat("Error: \"{0}\" caught because \"{1}\"", e.Message, e.InnerException.Message);
                else
                    m_logger.ErrorFormat("Error: {0}", e.Message);
                return new BooleanResult() { Success = false, Message = e.Message };
            }

            catch (Exception e)
            {
                m_logger.ErrorFormat("Error: {0}", e);
                return new BooleanResult { Success = false, Message = "Unspecified Error occurred. " + e.Message };
            }

        }

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }

        public void Starting() { }
        public void Stopping() { }

        /// <summary>
        /// Attempts to open a network stream to the specified server.
        /// </summary>
        /// <param name="server">Server address</param>
        /// <param name="port">Port number of server</param>
        /// <param name="ssl">Whether SSL is enabled</param>
        /// <returns>An open stream to the server.</returns>
        private Stream getNetworkStream(string server, int port, bool ssl)
        {   //Sets up network connection and returns the stream
            m_logger.DebugFormat("Connecting to {0}:{1}, {2} SSL", server, port, ssl ? "using" : "not using");
            TcpClient socket = new TcpClient(server, port);
            NetworkStream ns = socket.GetStream();

            ns.ReadTimeout = Settings.Store.NetworkTimeout;
            ns.WriteTimeout = Settings.Store.NetworkTimeout;

            if (ssl)
            {
                SslStream sns = new SslStream(ns, true);
                sns.AuthenticateAsClient(server);
                return sns;
            }
            return ns;
        }

        /// <summary>
        /// Authenticates against a POP3 server.
        /// If a timestamp is sent in the initial response, it assumes APOP is supported. 
        /// </summary>
        /// <param name="stream">Opened stream to POP3 server</param>
        /// <param name="creds">Username/password</param>
        /// <returns>True on success, false on failure</returns>
        private bool authPop3(System.IO.Stream stream, NetworkCredential creds)
        {
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(stream);
                System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);

                String resp = getResponse(reader);
                m_logger.DebugFormat("Initially connected: {0}", resp);

                if (!resp.StartsWith("+OK"))
                    throw new EMailAuthException("Invalid response from server.");

                //Check the server welcome message for a timestamp to indicate APOP support
                string apopHash = apopPass(resp, creds.Password);
                if (apopHash != null)
                {
                    writer.WriteLine(string.Format("APOP {0} {1}", creds.UserName, apopHash));
                    writer.Flush();

                    resp = getResponse(reader);
                    m_logger.DebugFormat("APOP response: {0}", resp);
                }

                //No timestamp = no APOP support, sending plaintext
                else
                {
                    writer.WriteLine("USER {0}", creds.UserName);
                    writer.Flush();

                    resp = getResponse(reader);
                    m_logger.DebugFormat("USER resp: {0}", resp);

                    writer.WriteLine("PASS {0}", creds.Password);
                    writer.Flush();

                    resp = getResponse(reader);
                    m_logger.DebugFormat("PASS resp: {0}", resp);
                }

                //Say goodbye to server
                writer.WriteLine("QUIT");
                writer.Flush();
                return resp.StartsWith("+OK");
            }
            catch (EMailAuthException){ throw; }
            catch (Exception e)
            {
                throw new EMailAuthException("Error communicating with POP server.", e);
            }
        }

        /// <summary>
        /// Authenticates against an IMAP server.
        /// Presently only supports plain text login. 
        /// The use of AUTHENTICATE is not yet supported, so SSL is advised.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="creds"></param>
        /// <returns></returns>
        private bool authImap(System.IO.Stream stream, NetworkCredential creds)
        {   //Sends username/password to IMAP server and verifies successful login.
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(stream);
                System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);

                String resp = getResponse(reader);
                m_logger.DebugFormat("IMAP server: {0}", resp);

                writer.WriteLine("li01 LOGIN {{{0}}}", creds.UserName.Length);
                writer.Flush();
                resp = getResponse(reader);
                m_logger.DebugFormat("IMAP Server: {0}", resp);
                if (!resp.StartsWith("+")) 
                    throw new EMailAuthException(string.Format("Unexpected response from server: {0}", resp));

                writer.WriteLine("{0} {{{1}}}", creds.UserName, creds.Password.Length);
                writer.Flush();
                resp = getResponse(reader);
                m_logger.DebugFormat("IMAP Server: {0}", resp);
                if (!resp.StartsWith("+"))
                    throw new EMailAuthException(string.Format("Unexpected response from server: {0}", resp));

                writer.WriteLine(creds.Password);
                writer.Flush();
                do
                {
                    resp = getResponse(reader);
                    m_logger.DebugFormat("IMAP Server: {0}", resp);
                } while (!resp.StartsWith("li01"));

                //Tell server we're disconnecting
                writer.WriteLine("lo01 LOGOUT");
                writer.Flush();

                return resp.StartsWith("li01 OK");
            }
            catch (EMailAuthException){ throw; }
            catch (Exception e)
            {
                throw new EMailAuthException("Error communicating with IMAP server.", e);
            }
        }

        /// <summary>
        /// Determines if the POP3 server supports APOP, and returns the authentication
        /// hash if so. 
        /// </summary>
        /// <param name="resp">Initial response from the POP3 server</param>
        /// <param name="password">Login password</param>
        /// <returns>MD5 password if APOP is supported, null otherwise.</returns>
        private string apopPass(string resp, string password)
        {
            //Determine if timestamp is present in the response
            int bIndex = resp.LastIndexOf('<');
            int eIndex = resp.LastIndexOf('>');

            if (bIndex < 0 || eIndex < 0 || eIndex < bIndex)
                return null;

            string timestamp = resp.Substring(bIndex, eIndex - bIndex + 1);

            MD5 md5 = MD5.Create();
            byte[] passbytes = System.Text.Encoding.UTF8.GetBytes(timestamp + password);
            byte[] hash = md5.ComputeHash(passbytes);

            //Convert byte[] to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Attempts to grab a response from the server.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Response from the server</returns>
        private string getResponse(System.IO.StreamReader reader)
        {   
            try
            {
                string output = null;
                output = reader.ReadLine();
                return output;
            }
            catch (IOException)
            {
                throw new EMailAuthException("IOException when reading response from server, possible timeout");
            }
            catch (Exception e)
            {
                throw new EMailAuthException("Error reading from server.", e);
            }
        }

    }

    public class EMailAuthException : Exception
    {
        public EMailAuthException(string message) : base(message) { }
        public EMailAuthException(string message, Exception inner) : base(message, inner) { }
    }
}
