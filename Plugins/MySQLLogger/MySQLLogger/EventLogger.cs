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

using pGina.Shared.Types;

namespace pGina.Plugin.MySqlLogger
{

    class EventLoggerMode : ILoggerMode
    {
        private ILog m_logger = LogManager.GetLogger("MySqlLoggerPlugin");
        public static readonly string UNKNOWN_USERNAME = "--Unknown--";

        public EventLoggerMode() { }

        public bool Log(System.ServiceProcess.SessionChangeDescription changeDescription, pGina.Shared.Types.SessionProperties properties)
        {
            string msg = null;
            switch (changeDescription.Reason)
            {
                case System.ServiceProcess.SessionChangeReason.SessionLogon:
                    msg = LogonEvent(changeDescription.SessionId, properties);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionLogoff:
                    msg = LogoffEvent(changeDescription.SessionId, properties);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionLock:
                    msg = SessionLockEvent(changeDescription.SessionId, properties);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionUnlock:
                    msg = SessionUnlockEvent(changeDescription.SessionId, properties);
                    break;
                case System.ServiceProcess.SessionChangeReason.SessionRemoteControl:
                    msg = SesionRemoteControlEvent(changeDescription.SessionId, properties);
                    break;
                case System.ServiceProcess.SessionChangeReason.ConsoleConnect:
                    msg = ConsoleConnectEvent(changeDescription.SessionId, properties);
                    break;
                case System.ServiceProcess.SessionChangeReason.ConsoleDisconnect:
                    msg = ConsoleDisconnectEvent(changeDescription.SessionId, properties);
                    break;
                case System.ServiceProcess.SessionChangeReason.RemoteConnect:
                    msg = RemoteConnectEvent(changeDescription.SessionId, properties);
                    break;
                case System.ServiceProcess.SessionChangeReason.RemoteDisconnect:
                    msg = RemoteDisconnectEvent(changeDescription.SessionId, properties);
                    break;
            }

            m_logger.DebugFormat("SessionChange({0}) - Message: {1}", changeDescription.Reason.ToString(), msg);

            if (!string.IsNullOrEmpty(msg))
            {
                m_logger.Debug(msg);

                // Log to DB
                try
                {
                    m_logger.Debug("Trying to log message.");
                    using (EventLogger log = EventLogger.Connect())
                    {
                        log.Log(msg);
                    }
                    return true;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 1042)
                        m_logger.ErrorFormat("Unable to connect to host: {0}", Settings.Store.Host);
                    else
                        m_logger.ErrorFormat("MySQL Exception: {0}", e.ToString());
                }
                catch (Exception e)
                {
                    m_logger.ErrorFormat("Error logging to DB: {0}", e);
                }

                return false;
            }
            return true; //No msg to log
        }

        public string TestTable()
        {
            string connStr = this.BuildConnectionString();
            if (connStr == null) return "Invalid server information.";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SHOW TABLES", conn);
                    string table = Settings.Store.Table;
                    bool tableExists = false;
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        if (Convert.ToString(rdr[0]) == table)
                            tableExists = true;
                    }
                    rdr.Close();
                    if (tableExists)
                        return "The table exists.";
                    else
                        return "Connection was successful, but no table exists.  Click \"Create Table\" to create the required table.";
                }
            }
            catch (MySqlException ex)
            {
                return String.Format("Connection failed: {0}", ex.Message);
            }
        }
        
        public string CreateTable()
        {
            string table = Settings.Store.Table;
            string connStr = this.BuildConnectionString();
            if (connStr == null) return "Invalid server information";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = string.Format(
                        "CREATE TABLE {0} (" +
                        "   TimeStamp DATETIME, " +
                        "   Host TINYTEXT, " +
                        "   Ip VARCHAR(15), " +
                        "   Machine TINYTEXT, " +
                        "   Message TEXT )", table);
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();

                    return "Table created.";
                }
            }
            catch (MySqlException ex)
            {
                return String.Format("Error: {0}", ex.Message);
            }
        }

        private string LogonEvent(int sessionId, SessionProperties properties)
        {
            bool okToLog = Settings.Store.EvtLogon;

            // Get the username
            string userName = getUsername(properties);

            // Since the username is not available at logoff time, we cache it
            // (tied to the session ID) so that we can get it back at the logoff
            // event.
            //if (userName != null)
            //    m_usernameCache.Add(sessionId, userName);
            if (userName == null)
                userName = UNKNOWN_USERNAME;

            if (okToLog)
                return string.Format("[{0}] Logon user: {1}", sessionId, userName);

            return "";
        }

        private string LogoffEvent(int sessionId, SessionProperties properties)
        {
            bool okToLog = Settings.Store.EvtLogoff;
            string userName = "";

            userName = getUsername(properties);
            // Delete the username from the cache because we are logging off?

            if (userName == null)
                userName = UNKNOWN_USERNAME;

            if (okToLog)
                return string.Format("[{0}] Logoff user: {1}", sessionId, userName);

            return "";
        }

        private string ConsoleConnectEvent(int sessionId, SessionProperties properties)
        {
            bool okToLog = Settings.Store.EvtConsoleConnect;
            string userName = "";

            userName = getUsername(properties);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Console connect user: {1}", sessionId, userName);

            return "";
        }

        private string ConsoleDisconnectEvent(int sessionId, SessionProperties properties)
        {
            bool okToLog = Settings.Store.EvtConsoleDisconnect;
            string userName = "";

            userName = getUsername(properties);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Console disconnect user: {1}", sessionId, userName);

            return "";
        }

        private string RemoteDisconnectEvent(int sessionId, SessionProperties properties)
        {
            bool okToLog = Settings.Store.EvtRemoteDisconnect;
            string userName = "";

            userName = getUsername(properties);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Remote disconnect user: {1}", sessionId, userName);

            return "";
        }

        private string RemoteConnectEvent(int sessionId, SessionProperties properties)
        {
            bool okToLog = Settings.Store.EvtRemoteConnect;
            string userName = "";

            userName = getUsername(properties);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Remote connect user: {1}", sessionId, userName);

            return "";
        }

        private string SesionRemoteControlEvent(int sessionId, SessionProperties properties)
        {
            bool okToLog = Settings.Store.EvtRemoteControl;
            string userName = "";

            userName = getUsername(properties);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Remote control user: {1}", sessionId, userName);

            return "";
        }

        private string SessionUnlockEvent(int sessionId, SessionProperties properties)
        {
            bool okToLog = Settings.Store.EvtUnlock;
            string userName = "";

            userName = getUsername(properties);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Session unlock user: {1}", sessionId, userName);

            return "";
        }

        private string SessionLockEvent(int sessionId, SessionProperties properties)
        {
            bool okToLog = Settings.Store.EvtLock;
            string userName = "";

            userName = getUsername(properties);
            if (userName == null)
                userName = UNKNOWN_USERNAME;


            if (okToLog)
                return string.Format("[{0}] Session lock user: {1}", sessionId, userName);

            return "";
        }

        private string getUsername(SessionProperties properties)
        {
            bool useModifiedName = Settings.Store.UseModifiedName;
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            if (useModifiedName)
                return userInfo.Username;
            else
                return userInfo.OriginalUsername;
            //return null;
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
            return bldr.GetConnectionString(true);

        }
    }

    class EventLogger : IDisposable
    {
        private MySqlConnection m_conn;
        private static string m_ip;
        private static string m_hostName;

        private static ILog m_logger = LogManager.GetLogger("DbLogger");
        private MySqlCommand m_command = null;

        static EventLogger()
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

        private EventLogger()
        {
            MySqlConnectionStringBuilder bldr = new MySqlConnectionStringBuilder();
            bldr.Server = Settings.Store.Host;
            bldr.Port = Convert.ToUInt32((string)Settings.Store.Port);
            bldr.UserID = Settings.Store.User;
            bldr.Database = Settings.Store.Database;
            bldr.Password = Settings.Store.GetEncryptedSetting("Password");
            string table = Settings.Store.Table;

            string connStr = bldr.GetConnectionString(true);
            m_logger.DebugFormat("Connecting to {0}:{1} as {2}, database: {3}", 
                bldr.Server, bldr.Port, bldr.UserID, bldr.Database);
            m_conn = new MySqlConnection(connStr);
            m_conn.Open();

            // Prepare statement
            m_logger.Debug("Prepare statement");
            string machine = Environment.MachineName;
            string sql = String.Format("INSERT INTO {0}(TimeStamp, Host, Ip, Machine, Message) " +
                "VALUES (NOW(), @host, @ip, @machine, @message)", table);
            m_command = new MySqlCommand(sql, m_conn);
            m_command.Prepare();
            m_command.Parameters.AddWithValue("@host", m_hostName);
            m_command.Parameters.AddWithValue("@ip", m_ip);
            m_command.Parameters.AddWithValue("@machine", machine);
            m_command.Parameters.Add("@message", MySqlDbType.Text);
        }

        public static EventLogger Connect()
        {
            return new EventLogger();
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
