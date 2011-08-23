using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace pGina.Plugin.MySqlLogger
{
    class DbLogger : IDisposable
    {
        private MySqlConnection m_conn;
        private static string m_ip;
        private static string m_hostName;

        private static DbLogger()
        {
            string m_hostName = Dns.GetHostName();
            IPHostEntry ipList = Dns.GetHostEntry(m_hostName);
            m_ip = ipList.AddressList[0].ToString();
        }

        private DbLogger()
        {
            string server = PluginImpl.Settings.Host;
            int port = PluginImpl.Settings.Port;
            string userName = PluginImpl.Settings.User;
            string passwd = PluginImpl.Settings.Password;
            string database = PluginImpl.Settings.Database;

            string connStr = String.Format("server={0}; port={1};user={2}; password={3}; database={4};",
                server, port, userName, passwd, database);
            m_conn = new MySqlConnection(connStr);
            m_conn.Open();
        }

        public static DbLogger Connect()
        {
            return new DbLogger();
        }

        public void Log(string message)
        {
            string tableName = PluginImpl.Settings.Table;
            string machine = Environment.MachineName;

            MySqlCommand command = new MySqlCommand(
                String.Format("INSERT INTO {0}(TimeStamp, Host, Ip, Machine, Message) VALUES (NOW(), {1}, {2}, {3}, {4})", 
                    tableName, m_hostName, m_ip, machine, message) );
            int rows = command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            m_conn.Close();
        }
    }
}
