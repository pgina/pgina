// OpenCredential - Source code
// https://github.com/pedropablobm/OpenCredential
//
// Copyright (c) 2024, Pedro Bermudez
// All rights reserved.
//
// This file is part of OpenCredential, an unofficial fork of pGina.
// It is distributed under the BSD-3-Clause license terms used by the
// original pGina project unless stated otherwise.
//
// For more details, see the LICENSE and NOTICE files included with
// this distribution.

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using log4net;
using Npgsql;
using OpenCredential.Shared.Types;

namespace OpenCredential.Plugin.DatabaseLogger
{
    class EventLogger : ILoggerMode
    {
        private readonly ILog m_logger = LogManager.GetLogger("DatabaseLoggerPlugin");
        public static readonly string UNKNOWN_USERNAME = "--Unknown--";
        private DbConnection m_conn;

        public bool Log(System.ServiceProcess.SessionChangeDescription changeDescription, SessionProperties properties)
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
                    msg = RemoteControlEvent(changeDescription.SessionId, properties);
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

            if (!string.IsNullOrEmpty(msg))
            {
                if (m_conn == null)
                    throw new InvalidOperationException("No database connection present.");

                LogToServer(msg);
            }

            return true;
        }

        public string TestTable()
        {
            EnsureConnection();

            try
            {
                string table = Convert.ToString(Settings.Store.EventTable);
                if (!TableExists(table))
                    return "Connection successful, but table does not exist. Click 'Create Table'.";

                string[] columns = { "TimeStamp", "Host", "Ip", "Machine", "Message" };
                using (var cmd = m_conn.CreateCommand())
                {
                    cmd.CommandText = IsPostgreSql
                        ? "SELECT column_name FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = @table ORDER BY ordinal_position"
                        : "DESCRIBE " + Quote(table);
                    if (IsPostgreSql)
                        AddParameter(cmd, "@table", table);

                    using (var rdr = cmd.ExecuteReader())
                    {
                        int colCt = 0;
                        while (rdr.Read())
                        {
                            string columnName = Convert.ToString(rdr[0]);
                            if (!columns.Contains(columnName))
                                return "Table exists but has invalid columns.";
                            colCt++;
                        }

                        return colCt == columns.Length ? "Table exists and is correct." : "Table has incorrect columns.";
                    }
                }
            }
            catch (Exception ex)
            {
                return string.Format("Connection failed: {0}", ex.Message);
            }
        }

        public string CreateTable()
        {
            EnsureConnection();

            try
            {
                string table = Convert.ToString(Settings.Store.EventTable);
                if (TableExists(table))
                    return "Table already exists.";

                string sql = IsPostgreSql
                    ? string.Format(
                        "CREATE TABLE {0} (\"TimeStamp\" TIMESTAMP NULL, \"Host\" VARCHAR(128) NULL, \"Ip\" VARCHAR(45) NULL, \"Machine\" VARCHAR(128) NULL, \"Message\" TEXT NULL)",
                        Quote(table))
                    : string.Format(
                        "CREATE TABLE {0} (TimeStamp DATETIME, Host TINYTEXT, Ip VARCHAR(45), Machine TINYTEXT, Message TEXT) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",
                        Quote(table));

                using (var cmd = m_conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }

                return "Table created.";
            }
            catch (Exception ex)
            {
                return string.Format("Error: {0}", ex.Message);
            }
        }

        public void SetConnection(DbConnection connection)
        {
            m_conn = connection;
        }

        private void LogToServer(string message)
        {
            EnsureConnection();
            string table = Convert.ToString(Settings.Store.EventTable);

            string sql = string.Format(
                "INSERT INTO {0}({1}, {2}, {3}, {4}, {5}) VALUES ({6}, @host, @ip, @machine, @message)",
                Quote(table),
                QuoteColumn("TimeStamp"),
                QuoteColumn("Host"),
                QuoteColumn("Ip"),
                QuoteColumn("Machine"),
                QuoteColumn("Message"),
                NowExpression);

            using (var cmd = m_conn.CreateCommand())
            {
                cmd.CommandText = sql;
                AddParameter(cmd, "@host", Dns.GetHostName());
                AddParameter(cmd, "@ip", GetIpAddress());
                AddParameter(cmd, "@machine", Environment.MachineName);
                AddParameter(cmd, "@message", message);
                cmd.ExecuteNonQuery();
            }
        }

        private bool TableExists(string table)
        {
            using (var cmd = m_conn.CreateCommand())
            {
                if (IsPostgreSql)
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = current_schema() AND table_name = @table";
                    AddParameter(cmd, "@table", table);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }

                cmd.CommandText = "SHOW TABLES LIKE @table";
                AddParameter(cmd, "@table", table);
                using (var rdr = cmd.ExecuteReader())
                    return rdr.Read();
            }
        }

        private void EnsureConnection()
        {
            if (m_conn == null)
                throw new InvalidOperationException("No database connection present.");

            if (m_conn.State != ConnectionState.Open)
                m_conn.Open();
        }

        private void AddParameter(DbCommand cmd, string name, object value)
        {
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(parameter);
        }

        private string Quote(string identifier)
        {
            return IsPostgreSql
                ? "\"" + identifier.Replace("\"", "\"\"") + "\""
                : "`" + identifier.Replace("`", "``") + "`";
        }

        private string QuoteColumn(string identifier)
        {
            return IsPostgreSql ? Quote(identifier) : identifier;
        }

        private bool IsPostgreSql
        {
            get { return m_conn is NpgsqlConnection; }
        }

        private string NowExpression
        {
            get { return IsPostgreSql ? "CURRENT_TIMESTAMP" : "NOW()"; }
        }

        private string GetIpAddress()
        {
            foreach (IPAddress addr in Dns.GetHostAddresses(string.Empty))
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return addr.ToString();
            return string.Empty;
        }

        private string GetUsername(int windowsSessionId, SessionProperties properties)
        {
            return SessionIdentityCache.ResolveUsername(
                windowsSessionId,
                properties,
                Settings.GetUseModifiedName(),
                UNKNOWN_USERNAME);
        }

        private string LogonEvent(int sessionId, SessionProperties properties)
        {
            return Settings.GetEvtLogon() ? string.Format("[{0}] Logon user: {1}", sessionId, GetUsername(sessionId, properties) ?? UNKNOWN_USERNAME) : string.Empty;
        }

        private string LogoffEvent(int sessionId, SessionProperties properties)
        {
            return Settings.GetEvtLogoff() ? string.Format("[{0}] Logoff user: {1}", sessionId, GetUsername(sessionId, properties) ?? UNKNOWN_USERNAME) : string.Empty;
        }

        private string ConsoleConnectEvent(int sessionId, SessionProperties properties)
        {
            return Settings.GetEvtConsoleConnect() ? string.Format("[{0}] Console connect", sessionId) : string.Empty;
        }

        private string ConsoleDisconnectEvent(int sessionId, SessionProperties properties)
        {
            return Settings.GetEvtConsoleDisconnect() ? string.Format("[{0}] Console disconnect", sessionId) : string.Empty;
        }

        private string RemoteDisconnectEvent(int sessionId, SessionProperties properties)
        {
            return Settings.GetEvtRemoteDisconnect() ? string.Format("[{0}] Remote disconnect user: {1}", sessionId, GetUsername(sessionId, properties) ?? UNKNOWN_USERNAME) : string.Empty;
        }

        private string RemoteConnectEvent(int sessionId, SessionProperties properties)
        {
            return Settings.GetEvtRemoteConnect() ? string.Format("[{0}] Remote connect user: {1}", sessionId, GetUsername(sessionId, properties) ?? UNKNOWN_USERNAME) : string.Empty;
        }

        private string RemoteControlEvent(int sessionId, SessionProperties properties)
        {
            return Settings.GetEvtRemoteControl() ? string.Format("[{0}] Remote control user: {1}", sessionId, GetUsername(sessionId, properties) ?? UNKNOWN_USERNAME) : string.Empty;
        }

        private string SessionUnlockEvent(int sessionId, SessionProperties properties)
        {
            return Settings.GetEvtUnlock() ? string.Format("[{0}] Session unlock user: {1}", sessionId, GetUsername(sessionId, properties) ?? UNKNOWN_USERNAME) : string.Empty;
        }

        private string SessionLockEvent(int sessionId, SessionProperties properties)
        {
            return Settings.GetEvtLock() ? string.Format("[{0}] Session lock user: {1}", sessionId, GetUsername(sessionId, properties) ?? UNKNOWN_USERNAME) : string.Empty;
        }
    }
}


