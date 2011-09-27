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

using MySql.Data;
using MySql.Data.MySqlClient;

using log4net;

namespace pGina.Plugin.MySqlLogger
{
    class DbLogger : IDisposable
    {
        private MySqlConnection m_conn;
        private static string m_ip;
        private static string m_hostName;

        private static ILog m_logger = LogManager.GetLogger("DbLogger");
        private MySqlCommand m_command = null;

        static DbLogger()
        {
            // Get host name and IP address of this computer
            m_hostName = Dns.GetHostName();
            IPAddress[] ipList = Dns.GetHostAddresses("");
            m_ip = "";
            // Grab the first IPv4 address in the list
            foreach (IPAddress addr in ipList)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    m_ip = addr.ToString();
                    break;
                }
            }
            m_logger.DebugFormat("My Host name: {0}", m_hostName);
            m_logger.DebugFormat("My IP: {0}", m_ip);
        }

        private DbLogger()
        {
            string server = Settings.Store.Host;
            int port = Settings.Store.Port;
            string userName = Settings.Store.User;
            string passwd = Settings.Store.GetEncryptedSetting("Password");
            string database = Settings.Store.Database;

            string connStr = String.Format("server={0}; port={1};user={2}; password={3}; database={4};",
                server, port, userName, passwd, database);
            m_logger.DebugFormat("Connecting to {0}:{1} as {2}, database: {3}", server, port, userName, database);
            m_conn = new MySqlConnection(connStr);
            m_conn.Open();

            // Prepare statement
            m_logger.Debug("Prepare statement");
            string machine = Environment.MachineName;
            string sql = String.Format("INSERT INTO {0}(TimeStamp, Host, Ip, Machine, Message) " +
                "VALUES (NOW(), @host, @ip, @machine, @message)", PluginImpl.TABLE_NAME);
            m_command = new MySqlCommand(sql, m_conn);
            m_command.Prepare();
            m_command.Parameters.AddWithValue("@host", m_hostName);
            m_command.Parameters.AddWithValue("@ip", m_ip);
            m_command.Parameters.AddWithValue("@machine", machine);
            m_command.Parameters.Add("@message", MySqlDbType.Text);
        }

        public static DbLogger Connect()
        {
            return new DbLogger();
        }

        public void Log(string message)
        {
            m_logger.DebugFormat("Logging: {0}", message);
            m_command.Parameters["@message"].Value = message;  
            int rows = m_command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            m_logger.Debug("Closing connection to MySQL.");
            m_conn.Close();
        }
    }
}
