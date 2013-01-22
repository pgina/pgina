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
    class SessionLogger : ILoggerMode
    {
        private ILog m_logger = LogManager.GetLogger("MySqlLoggerPlugin");
        private MySqlConnection m_conn;

        public SessionLogger(){ }

        //Logs the session if it's a LogOn or LogOff event.
        public bool Log(SessionChangeDescription changeDescription, SessionProperties properties)
        {
            if (m_conn == null)
                throw new InvalidOperationException("No MySQL Connection present.");

            string username = "--UNKNOWN--";
            if (properties != null)
            {
                UserInformation ui = properties.GetTrackedSingle<UserInformation>();
                if ((bool)Settings.Store.UseModifiedName)
                    username = ui.Username;
                else
                    username = ui.OriginalUsername;
            }

            //Logon Event
            if (changeDescription.Reason == SessionChangeReason.SessionLogon)
            {
                if(m_conn.State != System.Data.ConnectionState.Open)
                    m_conn.Open();

                string table = Settings.Store.SessionTable;
                
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

                m_logger.DebugFormat("Logged LogOn event for {0} at {1}", username, getIPAddress());

            }

            //LogOff Event
            else if (changeDescription.Reason == SessionChangeReason.SessionLogoff)
            {
                if (m_conn.State != System.Data.ConnectionState.Open)
                    m_conn.Open();

                string table = Settings.Store.SessionTable;

                string updatesql = string.Format("UPDATE {0} SET logoutstamp=NOW() "+
                    "WHERE logoutstamp=0 AND username=@username AND machine=@machine "+
                        "AND ipaddress=@ipaddress", table);

                MySqlCommand cmd = new MySqlCommand(updatesql, m_conn);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@machine", Environment.MachineName);
                cmd.Parameters.AddWithValue("@ipaddress", getIPAddress());
                cmd.ExecuteNonQuery();

                m_logger.DebugFormat("Logged LogOff event for {0} at {1}", username, getIPAddress());
            }

            return true;

        }

        //Tests the table based on the registry data. Returns a string indicating the table status.
        public string TestTable()
        {
            if (m_conn == null)
                throw new InvalidOperationException("No MySQL Connection present.");

            try
            {   //Open the connection if it's not presently open
                if (m_conn.State != System.Data.ConnectionState.Open)
                    m_conn.Open();

                string table = Settings.Store.SessionTable;
                MySqlCommand cmd = new MySqlCommand("SHOW TABLES", m_conn);
                
                bool tableExists = false;
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (Convert.ToString(rdr[0]) == table)
                        tableExists = true;
                }
                rdr.Close();
                
                if (!tableExists)
                    return "Connection was successful, but no table exists.  Click \"Create Table\" to create the required table.";

                //Table exists, verify columns
                string[] columns = { "dbid", "loginstamp", "logoutstamp", "username", "machine", "ipaddress"  };
                cmd = new MySqlCommand("DESCRIBE " + table, m_conn);
                rdr = cmd.ExecuteReader();
                int colCt = 0;
                
                //Check each column name and match it against the list of columns, keep count of how many match
                while (rdr.Read())
                {
                    string colName = Convert.ToString(rdr[0]);
                    if (!columns.Contains(colName))
                    {
                        rdr.Close();
                        return "Table exists, but has invalid columns.";
                    }
                    colCt++;
                }
                rdr.Close();

                //Check if each column was present
                if (colCt == columns.Length)
                    return "Table exists and is setup correctly.";
                else
                    return "Table exists, but appears to have incorrect columns.";
                
            }
            catch (MySqlException ex)
            {
                return String.Format("Error: {0}", ex.Message);
            }
        }

        //Creates the table based on the registry data. Returns a string indicating the table status.
        public string CreateTable()
        {
            if (m_conn == null)
                throw new InvalidOperationException("No MySQL Connection present.");

            try
            {
                if (m_conn.State != System.Data.ConnectionState.Open)
                    m_conn.Open();

                string table = Settings.Store.SessionTable;
                string sql = string.Format(
                    "CREATE TABLE {0} (" +
                    " dbid BIGINT NOT NULL AUTO_INCREMENT ," +
                    " loginstamp DATETIME NOT NULL, " +
                    " logoutstamp DATETIME NOT NULL, " +
                    " username TEXT NOT NULL, " +
                    " machine TEXT NOT NULL, " +
                    " ipaddress TEXT NOT NULL, " +
                    " INDEX (dbid))", table);

                MySqlCommand cmd = new MySqlCommand(sql, m_conn);
                cmd.ExecuteNonQuery();
                
                return "Table created.";
            }
            catch (MySqlException ex)
            {
                return String.Format("Error: {0}", ex.Message);
            }
        }

        //Provides the MySQL connection to use
        public void SetConnection(MySqlConnection m_conn)
        {
            this.m_conn = m_conn;
        }

        //Returns the IPv4 address of the current machine
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
    }
}
