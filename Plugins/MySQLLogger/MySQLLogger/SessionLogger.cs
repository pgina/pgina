using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ServiceProcess;

using log4net;

using MySql.Data.MySqlClient;

using pGina.Shared.Types;


namespace pGina.Plugin.MySqlLogger
{
    class SessionLogger : ILoggerMode, IDisposable
    {
        private ILog m_logger = LogManager.GetLogger("MySqlLoggerPlugin");
        private MySqlConnection m_conn;

        public SessionLogger(){ }

        public bool Log(SessionChangeDescription changeDescription, SessionProperties properties)
        {
            UserInformation ui = properties.GetTrackedSingle<UserInformation>();
            string username = "--UNKNOWN--";
            if((bool)Settings.Store.UseModifiedName)
                username = ui.Username;
            else
                username = ui.OriginalUsername;

            if (changeDescription.Reason == SessionChangeReason.SessionLogon)
            {
                string connStr = BuildConnectionString();
                m_conn = new MySqlConnection(connStr);
                m_conn.Open();

                string table = Settings.Store.Table;
                
                //Update the existing entry for this machine/ip if it exists.
                string updatesql = string.Format("UPDATE {0} SET logoutstamp=NOW() " +
                    "WHERE logoutstamp=0 and machine=@machine and ipaddress=@ipaddress", table);
                
                MySqlCommand cmd = new MySqlCommand(updatesql, m_conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@machine", Environment.MachineName);
                cmd.Parameters.AddWithValue("@ipaddress", getIPAddress());
                cmd.ExecuteNonQuery();
                
                //Insert new entry for this logon event
                string insertsql = string.Format("INSERT INTO {0} (dbid, loginstamp, logoutstamp, username,machine,ipaddress) " +
                    "VALUES (NULL, NOW(), 0, @username, @machine, @ipaddress)", table);
                cmd = new MySqlCommand(insertsql, m_conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@machine", Environment.MachineName);
                cmd.Parameters.AddWithValue("ipaddress", getIPAddress());
                cmd.ExecuteNonQuery();

                m_conn.Close();
                m_logger.DebugFormat("Logged LogOn event for {0} at {1}", username, getIPAddress());

            }

            else if (changeDescription.Reason == SessionChangeReason.SessionLogoff)
            {
                string connStr = BuildConnectionString();
                m_conn = new MySqlConnection(connStr);
                m_conn.Open();

                string table = Settings.Store.Table;

                string updatesql = string.Format("UPDATE {0} SET logoutstamp=NOW() "+
                    "WHERE logoutstamp=0 AND username=@username AND machine=@machine "+
                        "AND ipaddress=@ipaddress", table);

                MySqlCommand cmd = new MySqlCommand(updatesql, m_conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@machine", Environment.MachineName);
                cmd.Parameters.AddWithValue("@ipaddress", getIPAddress());
                cmd.ExecuteNonQuery();

                m_conn.Close();
                m_logger.DebugFormat("Logged LogOff event for {0} at {1}", username, getIPAddress());
            }

            return true;

        }

        public string TestTable()
        {
            try
            {
                string connStr = BuildConnectionString();
                m_conn = new MySqlConnection(connStr);
                m_conn.Open();

                string table = Settings.Store.Table;
                MySqlCommand cmd = new MySqlCommand("SHOW TABLES", m_conn);
                
                bool tableExists = false;
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (Convert.ToString(rdr[0]) == table)
                        tableExists = true;
                }
                rdr.Close();
                m_conn.Close();

                if (tableExists)
                    return "The table exists.";
                else
                    return "Connection was successful, but no table exists.  Click \"Create Table\" to create the required table.";
                
            }
            catch (MySqlException ex)
            {
                return String.Format("Error: {0}", ex.Message);
            }
        }

        public string CreateTable()
        {
            try
            {
                string connStr = BuildConnectionString();
                m_conn = new MySqlConnection(connStr);
                m_conn.Open();

                string table = Settings.Store.Table;
                string sql = string.Format(
                    "CREATE TABLE {0} (" +
                    "`dbid` BIGINT NOT NULL AUTO_INCREMENT ," +
                    "`loginstamp` DATETIME NOT NULL, " +
                    "`logoutstamp` DATETIME NOT NULL, " +
                    "`username` TEXT NOT NULL, " +
                    "`machine` TEXT NOT NULL, " +
                    "`ipaddress` TEXT NOT NULL, " +
                    "INDEX (`dbid`))", table);

                MySqlCommand cmd = new MySqlCommand(sql, m_conn);
                cmd.ExecuteNonQuery();
                
                m_conn.Close();
                return "Table created.";
            }
            catch (MySqlException ex)
            {
                return String.Format("Error: {0}", ex.Message);
            }
        }

        public void Dispose()
        {
            if(m_conn != null)
                m_conn.Close();
        }

        private string getIPAddress(){
            IPAddress[] ipList = Dns.GetHostAddresses("");
            
            // Grab the first IPv4 address in the list
            foreach (IPAddress addr in ipList)
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return addr.ToString();
                }
            }
            return "-INVALID IP ADDRESS-";
        }

        private string BuildConnectionString()
        {
            uint port = 0;
            try
            {
                port = Convert.ToUInt32((String)Settings.Store.Port);
            }
            catch (FormatException e)
            {
                throw new Exception("Invalid port number.", e);
            }

            MySqlConnectionStringBuilder bldr = new MySqlConnectionStringBuilder();
            bldr.Server = Settings.Store.Host;
            bldr.Port = port;
            bldr.UserID = Settings.Store.User;
            bldr.Database = Settings.Store.Database;
            bldr.Password = Settings.Store.GetEncryptedSetting("Password");
            m_logger.DebugFormat("Connecting to {0}:{1} as {2}, database: {3}",
                bldr.Server, bldr.Port, bldr.UserID, bldr.Database);
            return bldr.GetConnectionString(true);
        }
    }
}
