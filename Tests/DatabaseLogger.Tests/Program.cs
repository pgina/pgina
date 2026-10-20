using System;
using System.Data.Common;
using System.Reflection;
using Npgsql;
using OpenCredential.Plugin.DatabaseLogger;

internal static class Program
{
    private static int Main()
    {
        try
        {
            string value = Environment.GetEnvironmentVariable("OC_QA_LOGGER_CONNECTION");
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("Explicit synthetic PostgreSQL connection required.");
            using (var connection = new NpgsqlConnection(value))
            {
                connection.Open();
                Execute(connection, "SET TIME ZONE 'UTC'; CREATE TEMP TABLE oc_logger_sessions (loginstamp timestamp, logoutstamp timestamp, username text, machine text, ipaddress text, client_session_id text, windows_session_id int, session_state text, last_heartbeat_at timestamp, session_end_reason text, session_origin text NOT NULL DEFAULT 'online'); CREATE TEMP TABLE oc_logger_events (\"TimeStamp\" timestamp, \"Host\" text, \"Ip\" text, \"Machine\" text, \"Message\" text)");
                var now = DateTime.UtcNow;
                Replay(connection, "SessionLogon", now);
                Assert(Scalar(connection, "SELECT session_origin FROM oc_logger_sessions") == "offline_cache", "queued logon replay must preserve offline origin rather than inherit online default");
                Replay(connection, "Heartbeat", now.AddSeconds(60));
                Assert(Scalar(connection, "SELECT session_state FROM oc_logger_sessions") == "active", "heartbeat replay must keep session active");
                Replay(connection, "SessionLock", now.AddSeconds(65));
                Assert(Scalar(connection, "SELECT session_state FROM oc_logger_sessions") == "locked", "lock replay must update state");
                Replay(connection, "SessionUnlock", now.AddSeconds(70));
                Assert(Scalar(connection, "SELECT session_state FROM oc_logger_sessions") == "active", "unlock replay must restore state");
                Replay(connection, "SessionLogoff", now.AddSeconds(90));
                Assert(Scalar(connection, "SELECT count(*) FROM oc_logger_sessions WHERE logoutstamp IS NOT NULL AND session_state='ended' AND session_end_reason='logoff'") == "1", "logoff replay must close session");
                Assert(Scalar(connection, "SELECT session_origin FROM oc_logger_sessions") == "offline_cache", "heartbeat and close must not overwrite replayed origin");
                // A recovery closes the pending session even when the IP changed.
                Replay(connection, "SessionLogon", now.AddMinutes(2));
                Replay(connection, "SessionRecoveryEnd", now.AddMinutes(3), "192.0.2.99");
                Assert(Scalar(connection, "SELECT count(*) FROM oc_logger_sessions WHERE logoutstamp IS NULL") == "0", "recovery replay must close its session");
                Replay(connection, "SessionLogon", now.AddMinutes(4), null);
                Assert(Scalar(connection, "SELECT count(*) FROM oc_logger_sessions WHERE ipaddress IS NULL") == "1", "nullable IP must remain supported");
                Replay(connection, "SessionLogon", now, null, true);
                Assert(Scalar(connection, "SELECT count(*) FROM oc_logger_events") == "1", "event replay with null IP must succeed");
                Console.WriteLine("PASS Logger PostgreSQL: offline origin, logon, null end-reason heartbeat, lock, unlock, logoff, recovery and nullable event IP (temporary tables only).");
            }
            return 0;
        }
        catch (Exception ex)
        {
            while ((ex is TargetInvocationException || ex is TypeInitializationException) && ex.InnerException != null) ex = ex.InnerException;
            var pg = ex as PostgresException;
            // Never print connection strings, environment secrets or SQL payloads.
            Console.WriteLine("FAIL Logger: " + ex.GetType().Name + (pg != null ? " SQLSTATE=" + pg.SqlState : ""));
            if (ex is System.IO.FileLoadException load) Console.WriteLine("Assembly: " + load.FileName);
            if (ex is System.IO.FileNotFoundException missing) Console.WriteLine("Assembly: " + missing.FileName);
            if (ex is ReflectionTypeLoadException types)
                foreach (var loader in types.LoaderExceptions)
                    Console.WriteLine("Loader: " + loader.GetType().Name + ": " + (loader is System.IO.FileNotFoundException absent ? absent.FileName : loader is System.IO.FileLoadException invalid ? invalid.FileName : "dependency failure"));
            return 1;
        }
    }

    private static void Replay(NpgsqlConnection connection, string reason, DateTime utc, string ip = "192.0.2.1", bool asEvent = false)
    {
        var type = typeof(OfflineLogQueue).GetNestedType("QueuedLogEntry", BindingFlags.NonPublic);
        var entry = Activator.CreateInstance(type, true);
        Action<string, object> set = (name, item) => type.GetField(name).SetValue(entry, item);
        set("Mode", asEvent ? 0 : 1); set("Reason", reason); set("SessionId", 100);
        set("Username", "synthetic-logger"); set("Machine", "QA-ONLY"); set("IpAddress", ip);
        set("Message", asEvent ? "Synthetic test event" : null); set("EventUtc", utc);
        typeof(OfflineLogQueue).GetMethod("ReplayEntry", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new[] { (object)connection, entry });
    }
    private static void Execute(NpgsqlConnection connection, string sql) { using (var cmd = new NpgsqlCommand(sql, connection)) cmd.ExecuteNonQuery(); }
    private static string Scalar(NpgsqlConnection connection, string sql) { using (var cmd = new NpgsqlCommand(sql, connection)) return Convert.ToString(cmd.ExecuteScalar()); }
    private static void Assert(bool success, string message) { if (!success) throw new InvalidOperationException(message); }
}

namespace OpenCredential.Plugin.DatabaseLogger
{
    // Explicit test boundaries: never initialize real registry/plugin settings.
    internal enum LoggerMode { EVENT, SESSION }
    internal static class Settings
    {
        public static readonly TestStore Store = new TestStore();
        internal sealed class TestStore { public string EventTable = "oc_logger_events"; public string SessionTable = "oc_logger_sessions"; }
        public static string GetOfflineQueuePath() { throw new InvalidOperationException("SQLite not used in SQL replay regression."); }
        public static string GetPresenceStatePath() { return "unused"; }
        public static bool IsOfflineQueueEnabled() { return true; }
        public static int GetFlushBatchSize() { return 100; }
        public static string GetDatabaseProvider() { return "PostgreSql"; }
        public static bool IsPresenceTrackingEnabled() { return true; }
        public static int GetHeartbeatIntervalSeconds() { return 60; }
        public static int GetPresenceLeaseTimeoutSeconds() { return 300; }
        public static bool GetUseModifiedName() { return false; }
        public static bool GetEvtLogon() { return true; }
        public static bool GetEvtLogoff() { return true; }
        public static bool GetEvtLock() { return true; }
        public static bool GetEvtUnlock() { return true; }
        public static bool GetEvtRemoteControl() { return true; }
        public static bool GetEvtConsoleConnect() { return true; }
        public static bool GetEvtConsoleDisconnect() { return true; }
        public static bool GetEvtRemoteConnect() { return true; }
        public static bool GetEvtRemoteDisconnect() { return true; }
    }
    internal static class LoggerModeFactory
    {
        public static DbConnection CreateConnection() { throw new InvalidOperationException("Use explicit test connection."); }
        public static void OpenConnection(DbConnection connection) { throw new InvalidOperationException("Use explicit test connection."); }
    }
    internal static class SQLiteNativeBootstrap
    {
        public static void EnsureInitialized() { throw new InvalidOperationException("SQLite not used in SQL replay regression."); }
        public static string GetNativeDirectory() { return "unused"; }
        public static string GetNativeDllPath() { return "unused"; }
    }
    internal static class SessionIdentityCache
    {
        public static string ResolveUsername(int id, OpenCredential.Shared.Types.SessionProperties properties, bool modified, string fallback) { return "synthetic-logger"; }
    }
    internal static class SessionPresenceStore
    {
        public static void Initialize() { throw new InvalidOperationException("Presence store not used in SQL replay regression."); }
        public static int GetActiveSessionCount() { return 0; }
    }
    internal sealed class SessionPresenceState
    {
        public int WindowsSessionId { get; set; }
        public string Username { get; set; }
        public string Machine { get; set; }
        public string IpAddress { get; set; }
    }
}
