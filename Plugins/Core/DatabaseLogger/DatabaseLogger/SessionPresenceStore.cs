using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace OpenCredential.Plugin.DatabaseLogger
{
    internal sealed class SessionPresenceState
    {
        public int WindowsSessionId { get; set; }
        public string Username { get; set; }
        public string ClientSessionId { get; set; }
        public string Machine { get; set; }
        public string IpAddress { get; set; }
        public string SessionState { get; set; }
        public DateTime LoginAtUtc { get; set; }
        public DateTime LastHeartbeatUtc { get; set; }
        public bool WasOfflineLogon { get; set; }
        public bool SyncedToServer { get; set; }
    }

    internal static class SessionPresenceStore
    {
        private static readonly object SyncRoot = new object();

        public static void Initialize()
        {
            lock (SyncRoot)
            {
                SQLiteNativeBootstrap.EnsureInitialized();

                string dbPath = Settings.GetPresenceStatePath();
                string directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (!File.Exists(dbPath))
                    SQLiteConnection.CreateFile(dbPath);

                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "CREATE TABLE IF NOT EXISTS active_sessions (" +
                        "windows_session_id INTEGER PRIMARY KEY, " +
                        "username TEXT NULL, " +
                        "client_session_id TEXT NULL, " +
                        "machine TEXT NULL, " +
                        "ip_address TEXT NULL, " +
                        "session_state TEXT NOT NULL, " +
                        "login_at_utc TEXT NULL, " +
                        "last_heartbeat_utc TEXT NOT NULL, " +
                        "was_offline_logon INTEGER NOT NULL DEFAULT 0, " +
                        "synced_to_server INTEGER NOT NULL DEFAULT 1);";
                    cmd.ExecuteNonQuery();
                }

                EnsureColumn("active_sessions", "client_session_id", "TEXT NULL");
                EnsureColumn("active_sessions", "machine", "TEXT NULL");
                EnsureColumn("active_sessions", "ip_address", "TEXT NULL");
                EnsureColumn("active_sessions", "login_at_utc", "TEXT NULL");
                EnsureColumn("active_sessions", "was_offline_logon", "INTEGER NOT NULL DEFAULT 0");
                EnsureColumn("active_sessions", "synced_to_server", "INTEGER NOT NULL DEFAULT 1");
            }
        }

        public static void Upsert(SessionPresenceState state)
        {
            if (state == null)
                return;

            lock (SyncRoot)
            {
                Initialize();

                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO active_sessions (windows_session_id, username, client_session_id, machine, ip_address, session_state, login_at_utc, last_heartbeat_utc, was_offline_logon, synced_to_server) " +
                        "VALUES (@windows_session_id, @username, @client_session_id, @machine, @ip_address, @session_state, @login_at_utc, @last_heartbeat_utc, @was_offline_logon, @synced_to_server) " +
                        "ON CONFLICT(windows_session_id) DO UPDATE SET " +
                        "username = excluded.username, " +
                        "client_session_id = excluded.client_session_id, " +
                        "machine = excluded.machine, " +
                        "ip_address = excluded.ip_address, " +
                        "session_state = excluded.session_state, " +
                        "login_at_utc = excluded.login_at_utc, " +
                        "last_heartbeat_utc = excluded.last_heartbeat_utc, " +
                        "was_offline_logon = excluded.was_offline_logon, " +
                        "synced_to_server = excluded.synced_to_server;";
                    cmd.Parameters.AddWithValue("@windows_session_id", state.WindowsSessionId);
                    cmd.Parameters.AddWithValue("@username", (object)state.Username ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@client_session_id", (object)state.ClientSessionId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@machine", (object)state.Machine ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ip_address", (object)state.IpAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@session_state", string.IsNullOrWhiteSpace(state.SessionState) ? "active" : state.SessionState);
                    cmd.Parameters.AddWithValue("@login_at_utc", state.LoginAtUtc == default(DateTime)
                        ? DBNull.Value
                        : (object)state.LoginAtUtc.ToString("o", CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@last_heartbeat_utc", state.LastHeartbeatUtc.ToString("o", CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@was_offline_logon", state.WasOfflineLogon ? 1 : 0);
                    cmd.Parameters.AddWithValue("@synced_to_server", state.SyncedToServer ? 1 : 0);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<SessionPresenceState> LoadAll()
        {
            lock (SyncRoot)
            {
                Initialize();

                var states = new List<SessionPresenceState>();
                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT windows_session_id, username, client_session_id, machine, ip_address, session_state, login_at_utc, last_heartbeat_utc, was_offline_logon, synced_to_server " +
                        "FROM active_sessions";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            states.Add(new SessionPresenceState
                            {
                                WindowsSessionId = Convert.ToInt32(reader["windows_session_id"]),
                                Username = reader["username"] == DBNull.Value ? null : Convert.ToString(reader["username"]),
                                ClientSessionId = reader["client_session_id"] == DBNull.Value ? null : Convert.ToString(reader["client_session_id"]),
                                Machine = reader["machine"] == DBNull.Value ? null : Convert.ToString(reader["machine"]),
                                IpAddress = reader["ip_address"] == DBNull.Value ? null : Convert.ToString(reader["ip_address"]),
                                SessionState = Convert.ToString(reader["session_state"]),
                                LoginAtUtc = reader["login_at_utc"] == DBNull.Value
                                    ? DateTime.MinValue
                                    : DateTime.Parse(
                                        Convert.ToString(reader["login_at_utc"]),
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.RoundtripKind),
                                LastHeartbeatUtc = DateTime.Parse(
                                    Convert.ToString(reader["last_heartbeat_utc"]),
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.RoundtripKind),
                                WasOfflineLogon = reader["was_offline_logon"] != DBNull.Value && Convert.ToInt32(reader["was_offline_logon"]) != 0,
                                SyncedToServer = reader["synced_to_server"] == DBNull.Value || Convert.ToInt32(reader["synced_to_server"]) != 0
                            });
                        }
                    }
                }

                return states;
            }
        }

        public static int GetActiveSessionCount()
        {
            lock (SyncRoot)
            {
                Initialize();

                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM active_sessions";
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static string GetUsername(int windowsSessionId)
        {
            lock (SyncRoot)
            {
                Initialize();

                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT username FROM active_sessions WHERE windows_session_id = @windows_session_id LIMIT 1";
                    cmd.Parameters.AddWithValue("@windows_session_id", windowsSessionId);
                    object value = cmd.ExecuteScalar();
                    return value == null || value == DBNull.Value ? null : Convert.ToString(value);
                }
            }
        }

        public static void Remove(int windowsSessionId)
        {
            lock (SyncRoot)
            {
                Initialize();

                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM active_sessions WHERE windows_session_id = @windows_session_id";
                    cmd.Parameters.AddWithValue("@windows_session_id", windowsSessionId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static SQLiteConnection OpenConnection()
        {
            SQLiteNativeBootstrap.EnsureInitialized();
            var conn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", Settings.GetPresenceStatePath()));
            conn.Open();
            return conn;
        }

        private static void EnsureColumn(string tableName, string columnName, string definition)
        {
            using (var conn = OpenConnection())
            {
                if (HasColumn(conn, tableName, columnName))
                    return;

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = string.Format(
                        "ALTER TABLE {0} ADD COLUMN {1} {2}",
                        tableName,
                        columnName,
                        definition);
                    cmd.ExecuteNonQuery();
                }
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
    }
}
