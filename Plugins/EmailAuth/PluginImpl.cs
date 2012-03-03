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

using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
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
                m_logger.ErrorFormat("Format exception: {0}", e);
                return new BooleanResult() { Success = false, Message = "Port number is not valid." };
            }
            catch (IOException e)
            {   //Likely a read/write issue once connected.
                m_logger.ErrorFormat("IO Exception: {0}", e);
                return new BooleanResult() { Success = false, Message = "A Connection error occurred." };
            }

            catch (SocketException e)
            {   //Likely issue during initiated connection
                m_logger.ErrorFormat("Socket Exception error code: {0}, Trace: {1}", e.ErrorCode, e);
                return new BooleanResult() { Success = false, Message = "A connection error occurred." };
            }

            catch (Exception e)
            {
                m_logger.ErrorFormat("Error: {0}", e.Message);
                return new BooleanResult { Success = false, Message = "Unspecified Error occurred. " + e.Message };
            }
            /*catch (Exception e)
            {
                m_logger.ErrorFormat("AuthenticateUser exception: {0}", e);
                throw;  // Allow pGina service to catch and handle exception
            }*/
        }

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }

        public void Starting() { }
        public void Stopping() { }

        private string getResponse(System.IO.StreamReader reader)
        {   //Keeps trying to grab input. Will throw exception if connection error occurs.
            string output = null;
            do { output = reader.ReadLine(); }
            while (output == null);
            return output;
        }

        private Stream getNetworkStream(string server, int port, bool ssl)
        {   //Sets up network connection and returns the stream
            TcpClient socket = new TcpClient(server, port);
            NetworkStream ns = socket.GetStream();
            if (ssl)
            {
                SslStream sns = new SslStream(ns, true);
                m_logger.Debug("SSLStream opened, authenticating...");
                sns.AuthenticateAsClient(server);
                m_logger.DebugFormat("Authentication passed. Connected: {0}", socket.Connected);
                return sns;
            }

            m_logger.DebugFormat("Connected to server: {0}", socket.Connected);
            return ns;
        }

        private bool authPop3(System.IO.Stream stream, NetworkCredential creds)
        {   //Sends username/password to POP3 server and verifies succesful login.
            System.IO.StreamReader reader = new System.IO.StreamReader(stream);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);

            String resp = getResponse(reader);
            m_logger.DebugFormat("Initially connected: {0}", resp);

            writer.WriteLine("USER {0}", creds.UserName);
            writer.Flush();

            resp = getResponse(reader);
            m_logger.DebugFormat("USER resp: {0}", resp);

            writer.WriteLine("PASS {0}", creds.Password);
            writer.Flush();

            resp = getResponse(reader);
            m_logger.DebugFormat("PASS resp: {0}", resp);

            writer.WriteLine("QUIT");
            writer.Flush();
            return resp.StartsWith("+OK");
        }

        
        private bool authImap(System.IO.Stream stream, NetworkCredential creds)
        {   //Sends username/password to IMAP server and verifies successful login.
            System.IO.StreamReader reader = new System.IO.StreamReader(stream);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(stream);

            String resp = getResponse(reader);


            writer.WriteLine("li01 LOGIN {0} {1}", creds.UserName, creds.Password);
            writer.Flush();

            do //Read input until we get a response to li01 request
            {
                resp = getResponse(reader);
                m_logger.DebugFormat("Server response: {0}", resp);
            } while (!resp.StartsWith("li01"));

            //Tell server we're disconnecting
            writer.WriteLine("lo01 LOGOUT");
            writer.Flush();

            return resp.StartsWith("li01 OK");
        }

    }
}
