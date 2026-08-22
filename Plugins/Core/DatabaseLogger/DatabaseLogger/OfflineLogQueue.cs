using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using log4net;
using Npgsql;
using OpenCredential.Shared.Types;

namespace OpenCredential.Plugin.DatabaseLogger
{
    class OfflineLogQueue
    {
        private class QueuedLogEntry
        {
            public long Id;
            public int Mode;
            public string Reason;
            public string SessionState;
            public string SessionEndReason;
            public int SessionId;
            public string Username;
            public string Machine;
            public string IpAddress;
            public string Message;
            public DateTime EventUtc;
        }

        private static readonly ILog m_logger = LogManager.GetLogger("DatabaseLoggerPlugin.OfflineQueue");
        private static readonly object m_syncRoot = new object();

        public static void Initialize()
        {
            lock (m_syncRoot)
            {
                SQLiteNativeBootstrap.EnsureInitialized();

                string dbPath = Settings.GetOfflineQueuePath();
                string directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (!File.Exists(dbPath))
                    SQLiteConnection.CreateFile(dbPath);

                using (var conn = OpenConnection())
                {
                    EnsureSchema(conn);
                }
            }
        }

        public static void Enqueue(LoggerMode mode, System.ServiceProcess.SessionChangeDescription changeDescription, SessionProperties properties)
        {
            if (!Settings.IsOfflineQueueEnabled())
                return;

            lock (m_syncRoot)
            {
                Initialize();

                string username = GetUsername(changeDescription.SessionId, properties);
                string reason = changeDescription.Reason.ToString();
                string message = mode == LoggerMode.EVENT
                    ? BuildEventMessage(changeDescription.Reason, changeDescription.SessionId, username)
                    : null;

                if (mode == LoggerMode.EVENT && string.IsNullOrEmpty(message))
                    return;

                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO queued_logs (mode, reason, session_state, session_end_reason, session_id, username, machine, ip_address, message, event_utc) " +
                        "VALUES (@mode, @reason, @session_state, @session_end_reason, @session_id, @username, @machine, @ip_address, @message, @event_utc)";
                    cmd.Parameters.AddWithValue("@mode", (int)mode);
                    cmd.Parameters.AddWithValue("@reason", reason);
                    cmd.Parameters.AddWithValue("@session_state", DBNull.Value);
                    cmd.Parameters.AddWithValue("@session_end_reason", DBNull.Value);
                    cmd.Parameters.AddWithValue("@session_id", changeDescription.SessionId);
                    cmd.Parameters.AddWithValue("@username", (object)username ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@machine", Environment.MachineName);
                    cmd.Parameters.AddWithValue("@ip_address", (object)GetIpAddress() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@message", (object)message ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@event_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void FlushPending()
        {
            if (!Settings.IsOfflineQueueEnabled())
                return;

            lock (m_syncRoot)
            {
                Initialize();

                List<QueuedLogEntry> queuedEntries = ReadPending(Settings.GetFlushBatchSize());
                if (queuedEntries.Count == 0)
                    return;

                using (var dbConn = LoggerModeFactory.CreateConnection())
                {
                    dbConn.Open();

                    foreach (QueuedLogEntry entry in queuedEntries)
                    {
                        ReplayEntry(dbConn, entry);
                        DeleteEntry(entry.Id);
                    }
                }

                m_logger.InfoFormat("Flushed {0} offline log entries to {1}.", queuedEntries.Count, Settings.GetDatabaseProvider());
            }
        }

        private static List<QueuedLogEntry> ReadPending(int batchSize)
        {
            var entries = new List<QueuedLogEntry>();

            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT id, mode, reason, session_state, session_end_reason, session_id, username, machine, ip_address, message, event_utc " +
                    "FROM queued_logs ORDER BY id ASC LIMIT @limit";
                cmd.Parameters.AddWithValue("@limit", batchSize);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        entries.Add(new QueuedLogEntry
                        {
                            Id = Convert.ToInt64(reader["id"]),
                            Mode = Convert.ToInt32(reader["mode"]),
                            Reason = Convert.ToString(reader["reason"]),
                            SessionState = reader["session_state"] == DBNull.Value ? null : Convert.ToString(reader["session_state"]),
                            SessionEndReason = reader["session_end_reason"] == DBNull.Value ? null : Convert.ToString(reader["session_end_reason"]),
                            SessionId = Convert.ToInt32(reader["session_id"]),
                            Username = reader["username"] == DBNull.Value ? null : Convert.ToString(reader["username"]),
                            Machine = Convert.ToString(reader["machine"]),
                            IpAddress = reader["ip_address"] == DBNull.Value ? null : Convert.ToString(reader["ip_address"]),
                            Message = reader["message"] == DBNull.Value ? null : Convert.ToString(reader["message"]),
                            EventUtc = DateTime.Parse(Convert.ToString(reader["event_utc"]), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                        });
                    }
                }
            }

            return entries;
        }

        private static void ReplayEntry(DbConnection dbConn, QueuedLogEntry entry)
        {
            if (entry.Mode == (int)LoggerMode.EVENT)
            {
                ReplayEventLog(dbConn, entry);
                return;
            }

            ReplaySessionLog(dbConn, entry);
        }

        private static void ReplayEventLog(DbConnection dbConn, QueuedLogEntry entry)
        {
            string table = Convert.ToString(Settings.Store.EventTable);
            string sql = string.Format(
                "INSERT INTO {0}({1}, {2}, {3}, {4}, {5}) VALUES (@timeStamp, @host, @ip, @machine, @message)",
                Quote(table, dbConn),
                QuoteColumn("TimeStamp", dbConn),
                QuoteColumn("Host", dbConn),
                QuoteColumn("Ip", dbConn),
                QuoteColumn("Machine", dbConn),
                QuoteColumn("Message", dbConn));

            using (var cmd = dbConn.CreateCommand())
            {
                cmd.CommandText = sql;
                AddParameter(cmd, "@timeStamp", entry.EventUtc);
                AddParameter(cmd, "@host", Dns.GetHostName());
                AddParameter(cmd, "@ip", (object)entry.IpAddress ?? DBNull.Value);
                AddParameter(cmd, "@machine", entry.Machine);
                AddParameter(cmd, "@message", entry.Message ?? string.Empty);
                cmd.ExecuteNonQuery();
            }
        }

        private static void ReplaySessionLog(DbConnection dbConn, QueuedLogEntry entry)
        {
            string table = Convert.ToString(Settings.Store.SessionTable);
            bool closesSession =
                string.Equals(entry.Reason, "SessionLogoff", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(entry.Reason, "SessionRecoveryEnd", StringComparison.OrdinalIgnoreCase);

            string updateSql = string.Format(
                "UPDATE {0} SET {1}=@last_heartbeat_at, {2}=@session_state{3}{4} WHERE {5} IS NULL AND {6}=@machine AND (((@session_end_reason IS NOT NULL AND @session_end_reason <> '' AND {8} = @username) OR ({8} = @username AND {7}=@ipaddress)))",
                Quote(table, dbConn),
                QuoteColumn("last_heartbeat_at", dbConn),
                QuoteColumn("session_state", dbConn),
                closesSession
                    ? string.Format(", {0}=@logoutstamp", QuoteColumn("logoutstamp", dbConn))
                    : string.Empty,
                closesSession
                    ? string.Format(", {0}=@session_end_reason", QuoteColumn("session_end_reason", dbConn))
                    : string.Empty,
                QuoteColumn("logoutstamp", dbConn),
                QuoteColumn("machine", dbConn),
                QuoteColumn("ipaddress", dbConn),
                QuoteColumn("username", dbConn));

            if (string.Equals(entry.Reason, "SessionLogon", StringComparison.OrdinalIgnoreCase))
            {
                string insertSql = dbConn is NpgsqlConnection
                    ? string.Format(
                        "INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}) VALUES (@loginstamp, NULL, @username, @machine, @ipaddress, NULL, @windows_session_id, @session_state, @last_heartbeat_at, NULL)",
                        Quote(table, dbConn),
                        QuoteColumn("loginstamp", dbConn),
                        QuoteColumn("logoutstamp", dbConn),
                        QuoteColumn("username", dbConn),
                        QuoteColumn("machine", dbConn),
                        QuoteColumn("ipaddress", dbConn),
                        QuoteColumn("client_session_id", dbConn),
                        QuoteColumn("windows_session_id", dbConn),
                        QuoteColumn("session_state", dbConn),
                        QuoteColumn("last_heartbeat_at", dbConn),
                        QuoteColumn("session_end_reason", dbConn))
                    : string.Format(
                        "INSERT INTO {0} (dbid, loginstamp, logoutstamp, username, machine, ipaddress, client_session_id, windows_session_id, session_state, last_heartbeat_at, session_end_reason) VALUES (NULL, @loginstamp, NULL, @username, @machine, @ipaddress, NULL, @windows_session_id, @session_state, @last_heartbeat_at, NULL)",
                        Quote(table, dbConn));

                using (var insertCmd = dbConn.CreateCommand())
                {
                    insertCmd.CommandText = insertSql;
                    AddParameter(insertCmd, "@loginstamp", entry.EventUtc);
                    AddParameter(insertCmd, "@username", entry.Username ?? "--UNKNOWN--");
                    AddParameter(insertCmd, "@machine", entry.Machine);
                    AddParameter(insertCmd, "@ipaddress", (object)entry.IpAddress ?? DBNull.Value);
                    AddParameter(insertCmd, "@windows_session_id", entry.SessionId);
                    AddParameter(insertCmd, "@session_state", "active");
                    AddParameter(insertCmd, "@last_heartbeat_at", entry.EventUtc);
                    insertCmd.ExecuteNonQuery();
                }
            }
            else if (string.Equals(entry.Reason, "SessionLogoff", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(entry.Reason, "SessionRecoveryEnd", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(entry.Reason, "Heartbeat", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(entry.Reason, "SessionLock", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(entry.Reason, "SessionUnlock", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(entry.Reason, "ConsoleConnect", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(entry.Reason, "RemoteConnect", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(entry.Reason, "ConsoleDisconnect", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(entry.Reason, "RemoteDisconnect", StringComparison.OrdinalIgnoreCase))
            {
                using (var updateCmd = dbConn.CreateCommand())
                {
                    updateCmd.CommandText = updateSql;
                    AddParameter(updateCmd, "@last_heartbeat_at", entry.EventUtc);
                    AddParameter(updateCmd, "@session_state", ResolveSessionState(entry));
                    AddParameter(updateCmd, "@username", entry.Username ?? "--UNKNOWN--");
                    AddParameter(updateCmd, "@machine", entry.Machine);
                    AddParameter(updateCmd, "@ipaddress", (object)entry.IpAddress ?? DBNull.Value);
                    AddParameter(updateCmd, "@session_end_reason", closesSession
                        ? (object)ResolveSessionEndReason(entry)
                        : DBNull.Value);
                    if (closesSession)
                    {
                        AddParameter(updateCmd, "@logoutstamp", entry.EventUtc);
                    }
                    updateCmd.ExecuteNonQuery();
                }
            }
        }

        public static void EnqueueHeartbeat(int windowsSessionId, string username, string sessionState, DateTime heartbeatUtc)
        {
            if (!Settings.IsOfflineQueueEnabled())
                return;

            lock (m_syncRoot)
            {
                Initialize();

                using (var conn = OpenConnection())
                {
                    using (var deleteCmd = conn.CreateCommand())
                    {
                        deleteCmd.CommandText =
                            "DELETE FROM queued_logs WHERE mode = @mode AND reason = @reason AND session_id = @session_id";
                        deleteCmd.Parameters.AddWithValue("@mode", (int)LoggerMode.SESSION);
                        deleteCmd.Parameters.AddWithValue("@reason", "Heartbeat");
                        deleteCmd.Parameters.AddWithValue("@session_id", windowsSessionId);
                        deleteCmd.ExecuteNonQuery();
                    }

                    using (var insertCmd = conn.CreateCommand())
                    {
                        insertCmd.CommandText =
                            "INSERT INTO queued_logs (mode, reason, session_state, session_end_reason, session_id, username, machine, ip_address, message, event_utc) " +
                            "VALUES (@mode, @reason, @session_state, @session_end_reason, @session_id, @username, @machine, @ip_address, NULL, @event_utc)";
                        insertCmd.Parameters.AddWithValue("@mode", (int)LoggerMode.SESSION);
                        insertCmd.Parameters.AddWithValue("@reason", "Heartbeat");
                        insertCmd.Parameters.AddWithValue("@session_state", string.IsNullOrWhiteSpace(sessionState) ? "active" : sessionState);
                        insertCmd.Parameters.AddWithValue("@session_end_reason", DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@session_id", windowsSessionId);
                        insertCmd.Parameters.AddWithValue("@username", (object)username ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@machine", Environment.MachineName);
                        insertCmd.Parameters.AddWithValue("@ip_address", (object)GetIpAddress() ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@event_utc", heartbeatUtc.ToString("o", CultureInfo.InvariantCulture));
                        insertCmd.ExecuteNonQuery();
                    }

                    m_logger.DebugFormat(
                        "Queued heartbeat for session {0} user={1} state={2} at {3:o}.",
                        windowsSessionId,
                        username ?? "--UNKNOWN--",
                        string.IsNullOrWhiteSpace(sessionState) ? "active" : sessionState,
                        heartbeatUtc);
                }
            }
        }

        public static void EnqueueSessionRecovery(SessionPresenceState persistedState, DateTime eventUtc, string endReason)
        {
            if (!Settings.IsOfflineQueueEnabled() || persistedState == null)
                return;

            lock (m_syncRoot)
            {
                Initialize();

                using (var conn = OpenConnection())
                using (var insertCmd = conn.CreateCommand())
                {
                    insertCmd.CommandText =
                        "INSERT INTO queued_logs (mode, reason, session_state, session_end_reason, session_id, username, machine, ip_address, message, event_utc) " +
                        "VALUES (@mode, @reason, @session_state, @session_end_reason, @session_id, @username, @machine, @ip_address, NULL, @event_utc)";
                    insertCmd.Parameters.AddWithValue("@mode", (int)LoggerMode.SESSION);
                    insertCmd.Parameters.AddWithValue("@reason", "SessionRecoveryEnd");
                    insertCmd.Parameters.AddWithValue("@session_state", "ended");
                    insertCmd.Parameters.AddWithValue("@session_end_reason", endReason);
                    insertCmd.Parameters.AddWithValue("@session_id", persistedState.WindowsSessionId);
                    insertCmd.Parameters.AddWithValue("@username", (object)persistedState.Username ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@machine", (object)persistedState.Machine ?? Environment.MachineName);
                    insertCmd.Parameters.AddWithValue("@ip_address", (object)persistedState.IpAddress ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@event_utc", eventUtc.ToString("o", CultureInfo.InvariantCulture));
                    insertCmd.ExecuteNonQuery();
                }

                m_logger.InfoFormat(
                    "Queued recovered session close for session {0} user={1} reason={2} at {3:o}.",
                    persistedState.WindowsSessionId,
                    persistedState.Username ?? "--UNKNOWN--",
                    endReason,
                    eventUtc);
            }
        }

        private static string ResolveSessionState(QueuedLogEntry entry)
        {
            if (string.Equals(entry.Reason, "Heartbeat", StringComparison.OrdinalIgnoreCase))
                return string.IsNullOrWhiteSpace(entry.SessionState) ? "active" : entry.SessionState;

            switch (entry.Reason)
            {
                case "SessionLogoff":
                    return "ended";
                case "SessionLock":
                    return "locked";
                case "ConsoleDisconnect":
                case "RemoteDisconnect":
                    return "disconnected";
                default:
                    return "active";
            }
        }

        private static string ResolveSessionEndReason(QueuedLogEntry entry)
        {
            if (string.Equals(entry.Reason, "SessionRecoveryEnd", StringComparison.OrdinalIgnoreCase))
                return string.IsNullOrWhiteSpace(entry.SessionEndReason) ? "unexpected_shutdown" : entry.SessionEndReason;

            return "logoff";
        }

        private static void DeleteEntry(long id)
        {
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM queued_logs WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private static void EnsureSchema(SQLiteConnection conn)
        {
            using (var cmd = conn.CreateCommand())
            {
                    cmd.CommandText =
                        "CREATE TABLE IF NOT EXISTS queued_logs (" +
                        "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "mode INTEGER NOT NULL, " +
                        "reason TEXT NOT NULL, " +
                        "session_state TEXT NULL, " +
                        "session_end_reason TEXT NULL, " +
                        "session_id INTEGER NOT NULL, " +
                    "username TEXT NULL, " +
                    "machine TEXT NOT NULL, " +
                    "ip_address TEXT NULL, " +
                    "message TEXT NULL, " +
                    "event_utc TEXT NOT NULL);";
                cmd.ExecuteNonQuery();
            }

            if (HasColumn(conn, "queued_logs", "session_state"))
            {
                if (!HasColumn(conn, "queued_logs", "session_end_reason"))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "ALTER TABLE queued_logs ADD COLUMN session_end_reason TEXT NULL";
                        cmd.ExecuteNonQuery();
                    }
                }
                return;
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "ALTER TABLE queued_logs ADD COLUMN session_state TEXT NULL";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "ALTER TABLE queued_logs ADD COLUMN session_end_reason TEXT NULL";
                cmd.ExecuteNonQuery();
            }
        }

        private static bool HasColumn(SQLiteConnection conn, string tableName, string columnName)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info(" + tableName + ")";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.Equals(Convert.ToString(reader["name"]), columnName, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
            }

            return false;
        }

        public static string TestConfiguration()
        {
            var sb = new StringBuilder();
            string dbPath = Settings.GetOfflineQueuePath();
            string statePath = Settings.GetPresenceStatePath();
            string nativeDirectory = SQLiteNativeBootstrap.GetNativeDirectory();
            string nativeDllPath = SQLiteNativeBootstrap.GetNativeDllPath();

            sb.AppendLine("Offline queue");
            sb.AppendLine("-------------------------------");
            sb.AppendLine(string.Format("Process architecture: {0}", Environment.Is64BitProcess ? "x64" : "x86"));
            sb.AppendLine(string.Format("Native SQLite dir: {0}", nativeDirectory));
            sb.AppendLine(string.Format("Native SQLite dll: {0}", File.Exists(nativeDllPath) ? nativeDllPath : "MISSING"));
            sb.AppendLine(string.Format("Queue file: {0}", dbPath));
            sb.AppendLine(string.Format("Presence tracking enabled: {0}", Settings.IsPresenceTrackingEnabled() ? "Yes" : "No"));
            sb.AppendLine(string.Format("Heartbeat interval (secs): {0}", Settings.GetHeartbeatIntervalSeconds()));
            sb.AppendLine(string.Format("Presence lease timeout (secs): {0}", Settings.GetPresenceLeaseTimeoutSeconds()));
            sb.AppendLine(string.Format("State file: {0}", statePath));

            try
            {
                Initialize();
                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT 1";
                    cmd.ExecuteScalar();
                }

                sb.AppendLine("SQLite offline queue: OK");
                sb.AppendLine(string.Format("Queued items: {0}", GetQueuedItemCount()));
            }
            catch (Exception ex)
            {
                sb.AppendLine(string.Format("SQLite offline queue ERROR: {0}", ex.Message));
            }

            try
            {
                SessionPresenceStore.Initialize();
                sb.AppendLine(string.Format("Persisted active sessions: {0}", SessionPresenceStore.GetActiveSessionCount()));
            }
            catch (Exception ex)
            {
                sb.AppendLine(string.Format("Session presence store ERROR: {0}", ex.Message));
            }

            return sb.ToString();
        }

        private static int GetQueuedItemCount()
        {
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM queued_logs";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static SQLiteConnection OpenConnection()
        {
            SQLiteNativeBootstrap.EnsureInitialized();
            var conn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", Settings.GetOfflineQueuePath()));
            conn.Open();
            return conn;
        }

        private static string GetUsername(int windowsSessionId, SessionProperties properties)
        {
            return SessionIdentityCache.ResolveUsername(
                windowsSessionId,
                properties,
                Settings.GetUseModifiedName(),
                "--UNKNOWN--");
        }

        private static string GetIpAddress()
        {
            foreach (IPAddress addr in Dns.GetHostAddresses(string.Empty))
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return addr.ToString();
            }

            return string.Empty;
        }

        private static string BuildEventMessage(System.ServiceProcess.SessionChangeReason reason, int sessionId, string username)
        {
            switch (reason)
            {
                case System.ServiceProcess.SessionChangeReason.SessionLogon:
                    return Settings.GetEvtLogon() ? string.Format("[{0}] Logon user: {1}", sessionId, username ?? "--Unknown--") : string.Empty;
                case System.ServiceProcess.SessionChangeReason.SessionLogoff:
                    return Settings.GetEvtLogoff() ? string.Format("[{0}] Logoff user: {1}", sessionId, username ?? "--Unknown--") : string.Empty;
                case System.ServiceProcess.SessionChangeReason.SessionLock:
                    return Settings.GetEvtLock() ? string.Format("[{0}] Session lock user: {1}", sessionId, username ?? "--Unknown--") : string.Empty;
                case System.ServiceProcess.SessionChangeReason.SessionUnlock:
                    return Settings.GetEvtUnlock() ? string.Format("[{0}] Session unlock user: {1}", sessionId, username ?? "--Unknown--") : string.Empty;
                case System.ServiceProcess.SessionChangeReason.SessionRemoteControl:
                    return Settings.GetEvtRemoteControl() ? string.Format("[{0}] Remote control user: {1}", sessionId, username ?? "--Unknown--") : string.Empty;
                case System.ServiceProcess.SessionChangeReason.ConsoleConnect:
                    return Settings.GetEvtConsoleConnect() ? string.Format("[{0}] Console connect", sessionId) : string.Empty;
                case System.ServiceProcess.SessionChangeReason.ConsoleDisconnect:
                    return Settings.GetEvtConsoleDisconnect() ? string.Format("[{0}] Console disconnect", sessionId) : string.Empty;
                case System.ServiceProcess.SessionChangeReason.RemoteConnect:
                    return Settings.GetEvtRemoteConnect() ? string.Format("[{0}] Remote connect user: {1}", sessionId, username ?? "--Unknown--") : string.Empty;
                case System.ServiceProcess.SessionChangeReason.RemoteDisconnect:
                    return Settings.GetEvtRemoteDisconnect() ? string.Format("[{0}] Remote disconnect user: {1}", sessionId, username ?? "--Unknown--") : string.Empty;
                default:
                    return string.Empty;
            }
        }

        private static void AddParameter(DbCommand cmd, string name, object value)
        {
            var parameter = cmd.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(parameter);
        }

        private static string Quote(string identifier, DbConnection dbConn)
        {
            return dbConn is NpgsqlConnection
                ? "\"" + identifier.Replace("\"", "\"\"") + "\""
                : "`" + identifier.Replace("`", "``") + "`";
        }

        private static string QuoteColumn(string identifier, DbConnection dbConn)
        {
            return dbConn is NpgsqlConnection ? Quote(identifier, dbConn) : identifier;
        }
    }
}

