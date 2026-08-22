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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using log4net;
using Npgsql;
using OpenCredential.Shared.Types;

namespace OpenCredential.Plugin.DatabaseLogger
{
    class SessionLogger : ILoggerMode
    {
        private static readonly string[] RequiredColumns =
        {
            "dbid",
            "loginstamp",
            "logoutstamp",
            "username",
            "machine",
            "ipaddress",
            "client_session_id",
            "windows_session_id",
            "session_state",
            "last_heartbeat_at",
            "session_end_reason",
            "session_origin"
        };

        private readonly ILog m_logger = LogManager.GetLogger("DatabaseLoggerPlugin");
        private DbConnection m_conn;

        public bool Log(SessionChangeDescription changeDescription, SessionProperties properties)
        {
            EnsureConnection();
            EnsureSessionSchema();

            string username = ResolveUsername(changeDescription.SessionId, properties);
            string machine = Environment.MachineName;
            string ipAddress = GetIpAddress();
            string clientSessionId = GetClientSessionId(properties);
            DateTime eventUtc = DateTime.UtcNow;

            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLogon:
                    CloseCompetingSessions(changeDescription.SessionId, clientSessionId, username, machine, ipAddress, eventUtc);
                    InsertSession(changeDescription.SessionId, clientSessionId, username, machine, ipAddress, eventUtc, "active", "online");
                    m_logger.DebugFormat("Logged SessionLogon for {0} ({1})", username, clientSessionId);
                    break;

                case SessionChangeReason.SessionLogoff:
                    UpdateSessionPresence(changeDescription.SessionId, clientSessionId, username, machine, ipAddress, eventUtc, "ended", "logoff", true, "online");
                    m_logger.DebugFormat("Logged SessionLogoff for {0} ({1})", username, clientSessionId);
                    break;

                case SessionChangeReason.SessionLock:
                    UpdateSessionPresence(changeDescription.SessionId, clientSessionId, username, machine, ipAddress, eventUtc, "locked", null, false, "online");
                    break;

                case SessionChangeReason.SessionUnlock:
                case SessionChangeReason.ConsoleConnect:
                case SessionChangeReason.RemoteConnect:
                    UpdateSessionPresence(changeDescription.SessionId, clientSessionId, username, machine, ipAddress, eventUtc, "active", null, false, "online");
                    break;

                case SessionChangeReason.ConsoleDisconnect:
                case SessionChangeReason.RemoteDisconnect:
                    UpdateSessionPresence(changeDescription.SessionId, clientSessionId, username, machine, ipAddress, eventUtc, "disconnected", null, false, "online");
                    break;
            }

            return true;
        }

        public void WriteHeartbeat(int windowsSessionId, SessionProperties properties, string sessionState, DateTime heartbeatUtc)
        {
            EnsureConnection();
            EnsureSessionSchema();

            string username = ResolveUsername(windowsSessionId, properties);
            string machine = Environment.MachineName;
            string ipAddress = GetIpAddress();
            string clientSessionId = GetClientSessionId(properties);

            UpdateSessionPresence(windowsSessionId, clientSessionId, username, machine, ipAddress, heartbeatUtc, sessionState, null, false, "online");
            m_logger.DebugFormat(
                "Logged heartbeat for session {0} user={1} state={2} at {3:o}",
                windowsSessionId,
                username,
                string.IsNullOrWhiteSpace(sessionState) ? "active" : sessionState,
                heartbeatUtc);
        }

        public void ReconcileSessionEnd(SessionPresenceState persistedState, DateTime eventUtc, string endReason)
        {
            if (persistedState == null)
                return;

            EnsureConnection();
            EnsureSessionSchema();

            string machine = string.IsNullOrWhiteSpace(persistedState.Machine) ? Environment.MachineName : persistedState.Machine;
            string ipAddress = string.IsNullOrWhiteSpace(persistedState.IpAddress) ? GetIpAddress() : persistedState.IpAddress;
            string username = string.IsNullOrWhiteSpace(persistedState.Username) ? "--UNKNOWN--" : persistedState.Username;

            UpdateSessionPresence(
                persistedState.WindowsSessionId,
                persistedState.ClientSessionId,
                username,
                machine,
                ipAddress,
                eventUtc,
                "ended",
                endReason,
                true,
                persistedState.WasOfflineLogon ? "offline_cache" : "online");
            m_logger.InfoFormat(
                "Logged reconciled session end for session {0} user={1} reason={2} at {3:o}",
                persistedState.WindowsSessionId,
                username,
                endReason,
                eventUtc);
        }

        public void EnsureActiveSessionRecorded(SessionPresenceState persistedState, DateTime heartbeatUtc)
        {
            if (persistedState == null)
                return;

            EnsureConnection();
            EnsureSessionSchema();

            string username = string.IsNullOrWhiteSpace(persistedState.Username) ? "--UNKNOWN--" : persistedState.Username;
            string machine = string.IsNullOrWhiteSpace(persistedState.Machine) ? Environment.MachineName : persistedState.Machine;
            string ipAddress = string.IsNullOrWhiteSpace(persistedState.IpAddress) ? GetIpAddress() : persistedState.IpAddress;
            string sessionState = string.IsNullOrWhiteSpace(persistedState.SessionState) ? "active" : persistedState.SessionState;
            DateTime loginAtUtc = persistedState.LoginAtUtc == default(DateTime) ? heartbeatUtc : persistedState.LoginAtUtc;

            int updatedRows = UpdateSessionPresence(
                persistedState.WindowsSessionId,
                persistedState.ClientSessionId,
                username,
                machine,
                ipAddress,
                heartbeatUtc,
                sessionState,
                null,
                false,
                persistedState.WasOfflineLogon ? "offline_cache" : "online");

            if (updatedRows > 0)
                return;

            InsertSession(
                persistedState.WindowsSessionId,
                persistedState.ClientSessionId,
                username,
                machine,
                ipAddress,
                loginAtUtc,
                sessionState,
                persistedState.WasOfflineLogon ? "offline_cache" : "online");

            if (heartbeatUtc > loginAtUtc)
            {
                UpdateSessionPresence(
                    persistedState.WindowsSessionId,
                    persistedState.ClientSessionId,
                    username,
                    machine,
                    ipAddress,
                    heartbeatUtc,
                    sessionState,
                    null,
                    false,
                    persistedState.WasOfflineLogon ? "offline_cache" : "online");
            }
        }

        public string TestTable()
        {
            EnsureConnection();

            try
            {
                string table = Convert.ToString(Settings.Store.SessionTable);
                if (!TableExists(table))
                    return "Connection successful, but table does not exist. Click 'Create Table'.";

                List<string> existingColumns = GetExistingColumns(table);
                string[] missingColumns = RequiredColumns
                    .Where(col => !existingColumns.Contains(col, StringComparer.OrdinalIgnoreCase))
                    .ToArray();

                if (missingColumns.Length > 0)
                {
                    return string.Format(
                        "Table exists but is missing OpenCredential presence columns: {0}. Click 'Create Table' to update it.",
                        string.Join(", ", missingColumns));
                }

                return "Table exists and is correct.";
            }
            catch (Exception ex)
            {
                return string.Format("Error: {0}", ex.Message);
            }
        }

        public string CreateTable()
        {
            EnsureConnection();

            try
            {
                string table = Convert.ToString(Settings.Store.SessionTable);
                if (!TableExists(table))
                {
                    CreateSessionTable(table);
                    EnsureSessionIndexes(table);
                    return "Table created.";
                }

                EnsureSessionSchema();
                return "Table already exists and was updated.";
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

        private void EnsureConnection()
        {
            if (m_conn == null)
                throw new InvalidOperationException("No database connection present.");

            if (m_conn.State != ConnectionState.Open)
                m_conn.Open();
        }

        private void EnsureSessionSchema()
        {
            string table = Convert.ToString(Settings.Store.SessionTable);
            if (!TableExists(table))
                return;

            List<string> existingColumns = GetExistingColumns(table);
            EnsureColumn(table, existingColumns, "client_session_id", IsPostgreSql ? "VARCHAR(64) NULL" : "VARCHAR(64) NULL");
            EnsureColumn(table, existingColumns, "windows_session_id", "INT NULL");
            EnsureColumn(table, existingColumns, "session_state", IsPostgreSql ? "VARCHAR(32) NOT NULL DEFAULT 'active'" : "VARCHAR(32) NOT NULL DEFAULT 'active'");
            EnsureColumn(table, existingColumns, "last_heartbeat_at", IsPostgreSql ? "TIMESTAMP NULL" : "DATETIME NULL");
            EnsureColumn(table, existingColumns, "session_end_reason", IsPostgreSql ? "VARCHAR(64) NULL" : "VARCHAR(64) NULL");
            EnsureColumn(table, existingColumns, "session_origin", IsPostgreSql ? "VARCHAR(32) NOT NULL DEFAULT 'online'" : "VARCHAR(32) NOT NULL DEFAULT 'online'");
            EnsureSessionIndexes(table);
        }

        private void EnsureColumn(string table, List<string> existingColumns, string columnName, string columnDefinition)
        {
            if (existingColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
                return;

            using (var cmd = m_conn.CreateCommand())
            {
                cmd.CommandText = string.Format(
                    "ALTER TABLE {0} ADD COLUMN {1} {2}",
                    Quote(table),
                    QuoteColumn(columnName),
                    columnDefinition);
                cmd.ExecuteNonQuery();
            }

            existingColumns.Add(columnName);
        }

        private void EnsureSessionIndexes(string table)
        {
            EnsureIndex(table, "idx_" + table + "_client_session", new[] { "client_session_id" });
            EnsureIndex(table, "idx_" + table + "_presence", new[] { "logoutstamp", "last_heartbeat_at", "session_state" });
        }

        private void EnsureIndex(string table, string indexName, string[] columns)
        {
            if (IndexExists(table, indexName))
                return;

            string quotedColumns = string.Join(", ", columns.Select(QuoteColumn));
            using (var cmd = m_conn.CreateCommand())
            {
                cmd.CommandText = string.Format(
                    "CREATE INDEX {0} ON {1} ({2})",
                    Quote(indexName),
                    Quote(table),
                    quotedColumns);
                cmd.ExecuteNonQuery();
            }
        }

        private bool IndexExists(string table, string indexName)
        {
            using (var cmd = m_conn.CreateCommand())
            {
                if (IsPostgreSql)
                {
                    cmd.CommandText =
                        "SELECT COUNT(*) FROM pg_indexes WHERE schemaname = current_schema() AND tablename = @table AND indexname = @index";
                    AddParameter(cmd, "@table", table);
                    AddParameter(cmd, "@index", indexName);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }

                cmd.CommandText =
                    "SELECT COUNT(*) FROM information_schema.statistics WHERE table_schema = DATABASE() AND table_name = @table AND index_name = @index";
                AddParameter(cmd, "@table", table);
                AddParameter(cmd, "@index", indexName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
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

        private List<string> GetExistingColumns(string table)
        {
            using (var cmd = m_conn.CreateCommand())
            {
                cmd.CommandText = IsPostgreSql
                    ? "SELECT column_name FROM information_schema.columns WHERE table_schema = current_schema() AND table_name = @table ORDER BY ordinal_position"
                    : "SELECT column_name FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = @table ORDER BY ordinal_position";
                AddParameter(cmd, "@table", table);

                var columns = new List<string>();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        columns.Add(Convert.ToString(rdr[0]));
                }

                return columns;
            }
        }

        private void CreateSessionTable(string table)
        {
            string sql = IsPostgreSql
                ? string.Format(
                    "CREATE TABLE {0} (\"dbid\" BIGSERIAL PRIMARY KEY, \"loginstamp\" TIMESTAMP NOT NULL, \"logoutstamp\" TIMESTAMP NULL, \"username\" VARCHAR(128) NOT NULL, \"machine\" VARCHAR(128) NOT NULL, \"ipaddress\" VARCHAR(45) NOT NULL, \"client_session_id\" VARCHAR(64) NULL, \"windows_session_id\" INT NULL, \"session_state\" VARCHAR(32) NOT NULL DEFAULT 'active', \"last_heartbeat_at\" TIMESTAMP NULL, \"session_end_reason\" VARCHAR(64) NULL, \"session_origin\" VARCHAR(32) NOT NULL DEFAULT 'online')",
                    Quote(table))
                : string.Format(
                    "CREATE TABLE {0} (dbid BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY, loginstamp DATETIME NOT NULL, logoutstamp DATETIME NULL, username VARCHAR(128) NOT NULL, machine VARCHAR(128) NOT NULL, ipaddress VARCHAR(45) NOT NULL, client_session_id VARCHAR(64) NULL, windows_session_id INT NULL, session_state VARCHAR(32) NOT NULL DEFAULT 'active', last_heartbeat_at DATETIME NULL, session_end_reason VARCHAR(64) NULL, session_origin VARCHAR(32) NOT NULL DEFAULT 'online', INDEX idx_{1}_active (logoutstamp, machine, ipaddress), INDEX idx_{1}_user (username, machine, ipaddress)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4",
                    Quote(table),
                    table);

            using (var cmd = m_conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertSession(int windowsSessionId, string clientSessionId, string username, string machine, string ipAddress, DateTime eventUtc, string sessionState, string sessionOrigin)
        {
            string table = Convert.ToString(Settings.Store.SessionTable);
            string sql = string.Format(
                "INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}) VALUES (@loginstamp, NULL, @username, @machine, @ipaddress, @client_session_id, @windows_session_id, @session_state, @last_heartbeat_at, NULL, @session_origin)",
                Quote(table),
                QuoteColumn("loginstamp"),
                QuoteColumn("logoutstamp"),
                QuoteColumn("username"),
                QuoteColumn("machine"),
                QuoteColumn("ipaddress"),
                QuoteColumn("client_session_id"),
                QuoteColumn("windows_session_id"),
                QuoteColumn("session_state"),
                QuoteColumn("last_heartbeat_at"),
                QuoteColumn("session_end_reason"),
                QuoteColumn("session_origin"));

            using (var cmd = m_conn.CreateCommand())
            {
                cmd.CommandText = sql;
                AddParameter(cmd, "@loginstamp", eventUtc);
                AddParameter(cmd, "@username", username);
                AddParameter(cmd, "@machine", machine);
                AddParameter(cmd, "@ipaddress", ipAddress);
                AddParameter(cmd, "@client_session_id", NullableDbValue(clientSessionId));
                AddParameter(cmd, "@windows_session_id", windowsSessionId);
                AddParameter(cmd, "@session_state", sessionState);
                AddParameter(cmd, "@last_heartbeat_at", eventUtc);
                AddParameter(cmd, "@session_origin", string.IsNullOrWhiteSpace(sessionOrigin) ? "online" : sessionOrigin);
                cmd.ExecuteNonQuery();
            }
        }

        private void CloseCompetingSessions(int windowsSessionId, string clientSessionId, string username, string machine, string ipAddress, DateTime eventUtc)
        {
            UpdateSessionPresence(windowsSessionId, clientSessionId, username, machine, ipAddress, eventUtc, "ended", "superseded_by_logon", true, "online");
        }

        private int UpdateSessionPresence(
            int windowsSessionId,
            string clientSessionId,
            string username,
            string machine,
            string ipAddress,
            DateTime heartbeatUtc,
            string sessionState,
            string endReason,
            bool closeSession,
            string sessionOrigin)
        {
            string table = Convert.ToString(Settings.Store.SessionTable);
            string normalizedClientSessionId = clientSessionId ?? string.Empty;
            string sql = string.Format(
                "UPDATE {0} SET {1} = @last_heartbeat_at, {2} = @session_state, {3} = CASE WHEN ({4} IS NULL OR {4} = '') THEN {3} ELSE @client_session_id END, {5} = CASE WHEN {5} IS NULL THEN @windows_session_id ELSE {5} END, {6} = CASE WHEN ({6} IS NULL OR {6} = '') THEN @session_origin ELSE {6} END{7}{8} WHERE {9} IS NULL AND {10} = @machine AND (((@client_session_id IS NOT NULL AND @client_session_id <> '') AND {3} = @client_session_id) OR ({5} = @windows_session_id) OR ({11} = @username AND {12} = @ipaddress))",
                Quote(table),
                QuoteColumn("last_heartbeat_at"),
                QuoteColumn("session_state"),
                QuoteColumn("client_session_id"),
                QuoteColumn("client_session_id"),
                QuoteColumn("windows_session_id"),
                QuoteColumn("session_origin"),
                closeSession ? string.Format(", {0} = @logoutstamp", QuoteColumn("logoutstamp")) : string.Empty,
                closeSession ? string.Format(", {0} = @session_end_reason", QuoteColumn("session_end_reason")) : string.Empty,
                QuoteColumn("logoutstamp"),
                QuoteColumn("machine"),
                QuoteColumn("username"),
                QuoteColumn("ipaddress"));

            using (var cmd = m_conn.CreateCommand())
            {
                cmd.CommandText = sql;
                AddParameter(cmd, "@last_heartbeat_at", heartbeatUtc);
                AddParameter(cmd, "@session_state", sessionState);
                AddParameter(cmd, "@client_session_id", normalizedClientSessionId);
                AddParameter(cmd, "@windows_session_id", windowsSessionId);
                AddParameter(cmd, "@machine", machine);
                AddParameter(cmd, "@username", username);
                AddParameter(cmd, "@ipaddress", ipAddress);
                AddParameter(cmd, "@session_origin", string.IsNullOrWhiteSpace(sessionOrigin) ? "online" : sessionOrigin);
                if (closeSession)
                {
                    AddParameter(cmd, "@logoutstamp", heartbeatUtc);
                    AddParameter(cmd, "@session_end_reason", NullableDbValue(endReason));
                }
                return cmd.ExecuteNonQuery();
            }
        }

        private string GetClientSessionId(SessionProperties properties)
        {
            if (properties == null || properties.Id == Guid.Empty)
                return null;

            return properties.Id.ToString("D");
        }

        private string ResolveUsername(int windowsSessionId, SessionProperties properties)
        {
            return SessionIdentityCache.ResolveUsername(
                windowsSessionId,
                properties,
                Settings.GetUseModifiedName(),
                "--UNKNOWN--");
        }

        private string GetIpAddress()
        {
            foreach (IPAddress addr in Dns.GetHostAddresses(string.Empty))
            {
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return addr.ToString();
            }

            return string.Empty;
        }

        private object NullableDbValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;
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
    }
}
