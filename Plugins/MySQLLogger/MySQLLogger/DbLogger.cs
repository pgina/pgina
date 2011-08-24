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

        private ILog m_logger = LogManager.GetLogger("DbLogger");
        private MySqlCommand m_command = null;

        static DbLogger()
        {
            // Get host name and IP address of this computer
            m_hostName = Dns.GetHostName();
            IPAddress[] ipList = Dns.GetHostAddresses("");
            m_ip = "";
            foreach (IPAddress addr in ipList)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    m_ip = addr.ToString();
                    break;
                }
            }
        }

        private DbLogger()
        {
            m_logger.DebugFormat("My Host name: {0}", m_hostName);
            m_logger.DebugFormat("My IP: {0}", m_ip);

            string server = PluginImpl.Settings.Host;
            int port = PluginImpl.Settings.Port;
            string userName = PluginImpl.Settings.User;
            string passwd = PluginImpl.Settings.Password;
            string database = PluginImpl.Settings.Database;

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
