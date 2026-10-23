using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Principal;
using log4net;

namespace OpenCredential.Plugin.DatabaseAuth
{
    class LocalUserCache
    {
        private static readonly ILog m_logger = LogManager.GetLogger("DatabaseAuth.LocalUserCache");
        private static readonly object m_syncRoot = new object();
        private static readonly HashSet<string> m_securedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static void Initialize()
        {
            lock (m_syncRoot)
            {
                SQLiteNativeBootstrap.EnsureInitialized();

                string dbPath = Settings.GetLocalCachePath();
                string directory = Path.GetDirectoryName(dbPath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(dbPath))
                {
                    SQLiteConnection.CreateFile(dbPath);
                }

                if (!m_securedPaths.Contains(dbPath))
                {
                    var security = new FileSecurity();
                    security.SetAccessRuleProtection(true, false);
                    foreach (var sid in new[] { WindowsIdentity.GetCurrent().User,
                        new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                        new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null) })
                        security.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.FullControl, AccessControlType.Allow));
                    File.SetAccessControl(dbPath, security);
                    m_securedPaths.Add(dbPath);
                }

                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "CREATE TABLE IF NOT EXISTS users (" +
                        "username TEXT PRIMARY KEY, " +
                        "hash_method TEXT NOT NULL, " +
                        "hash_value TEXT NOT NULL, " +
                        "status_value TEXT NULL, " +
                        "failed_attempts INTEGER NOT NULL DEFAULT 0, " +
                        "blocked_until_utc TEXT NULL, " +
                        "updated_utc TEXT NOT NULL);" +
                        "CREATE TABLE IF NOT EXISTS metadata (" +
                        "key TEXT PRIMARY KEY, " +
                        "value TEXT NULL);";
                    cmd.ExecuteNonQuery();
                }

                EnsureOptionalColumns();
            }
        }

        public static void UpsertUser(UserEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Name))
                return;

            UpsertUsers(new[] { entry });
        }

        public static void UpsertUsers(IEnumerable<UserEntry> entries)
        {
            WriteUsers(entries, false);
        }

        public static void ReplaceUsers(IEnumerable<UserEntry> entries)
        {
            if (entries == null) throw new ArgumentNullException("entries");
            WriteUsers(entries, true);
        }

        private static void WriteUsers(IEnumerable<UserEntry> entries, bool replace)
        {
            if (entries == null)
                return;

            // Materialize and validate the entire response before touching the previous snapshot.
            var snapshot = entries.ToList();
            if (replace && (snapshot.Any(entry => entry == null || string.IsNullOrWhiteSpace(entry.Name)) ||
                snapshot.GroupBy(entry => entry.Name, StringComparer.Ordinal).Any(group => group.Count() > 1)))
                throw new InvalidOperationException("The complete user snapshot contains invalid or duplicate identities.");

            lock (m_syncRoot)
            {
                Initialize();

                using (var conn = OpenConnection())
                using (var transaction = conn.BeginTransaction())
                {
                    if (replace)
                    {
                        using (var clear = conn.CreateCommand())
                        {
                            clear.Transaction = transaction;
                            clear.CommandText = "DELETE FROM users";
                            clear.ExecuteNonQuery();
                        }
                    }
                    DateTime writtenUtc = DateTime.UtcNow;
                    foreach (UserEntry entry in snapshot)
                    {
                        if (entry == null || string.IsNullOrWhiteSpace(entry.Name))
                            continue;

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandText =
                                "INSERT INTO users (username, hash_method, hash_value, status_value, failed_attempts, blocked_until_utc, updated_utc) " +
                                "VALUES (@username, @hash_method, @hash_value, @status_value, @failed_attempts, @blocked_until_utc, @updated_utc) " +
                                "ON CONFLICT(username) DO UPDATE SET " +
                                "hash_method = excluded.hash_method, " +
                                "hash_value = excluded.hash_value, " +
                                "status_value = excluded.status_value, " +
                                "failed_attempts = excluded.failed_attempts, " +
                                "blocked_until_utc = excluded.blocked_until_utc, " +
                                "updated_utc = excluded.updated_utc;";
                            cmd.Parameters.AddWithValue("@username", entry.Name);
                            cmd.Parameters.AddWithValue("@hash_method", entry.HashAlg.ToString());
                            cmd.Parameters.AddWithValue("@hash_value", ProtectHash(entry.Hash ?? string.Empty));
                            cmd.Parameters.AddWithValue("@status_value", (object)entry.StatusValue ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@failed_attempts", entry.FailedAttempts);
                            cmd.Parameters.AddWithValue(
                                "@blocked_until_utc",
                                entry.BlockedUntilUtc.HasValue
                                    ? (object)entry.BlockedUntilUtc.Value.ToString("o", CultureInfo.InvariantCulture)
                                    : DBNull.Value);
                            cmd.Parameters.AddWithValue("@updated_utc", writtenUtc.ToString("o", CultureInfo.InvariantCulture));
                            cmd.ExecuteNonQuery();
                        }
                    }

                    if (replace)
                    {
                        WriteMetadata(conn, transaction, "last_sync_utc", writtenUtc.ToString("o", CultureInfo.InvariantCulture));
                        WriteMetadata(conn, transaction, "last_attempt_utc", writtenUtc.ToString("o", CultureInfo.InvariantCulture));
                        WriteMetadata(conn, transaction, "last_attempt_result", "completed");
                    }
                    transaction.Commit();
                }
            }
        }

        public static bool TryAuthenticate(string username, string password, out UserEntry cachedEntry)
        {
            cachedEntry = null;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
                return false;

            lock (m_syncRoot)
            {
                Initialize();

                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT hash_method, hash_value, status_value, failed_attempts, blocked_until_utc, updated_utc FROM users WHERE username = @username";
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        string hashMethod = Convert.ToString(reader["hash_method"]);
                        string hashValue = Convert.ToString(reader["hash_value"]);
                        if (hashValue.StartsWith("dpapi:", StringComparison.Ordinal))
                            hashValue = Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(hashValue.Substring(6)), null, DataProtectionScope.LocalMachine));
                        string statusValue = reader["status_value"] == DBNull.Value
                            ? null
                            : Convert.ToString(reader["status_value"]);
                        int failedAttempts = reader["failed_attempts"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["failed_attempts"]);
                        DateTime? blockedUntilUtc = ParseUtc(reader["blocked_until_utc"]);
                        DateTime? updatedUtc = ParseUtc(reader["updated_utc"]);
                        if (!updatedUtc.HasValue || updatedUtc.Value > DateTime.UtcNow.AddMinutes(2) ||
                            DateTime.UtcNow - updatedUtc.Value > TimeSpan.FromHours(Settings.GetOfflineCacheMaxAgeHours()))
                            return false;
                        PasswordHashAlgorithm hashAlg;

                        if (!TryParseHashAlgorithm(hashMethod, out hashAlg))
                        {
                            m_logger.WarnFormat("Cached user {0} has unknown hash algorithm '{1}'", username, hashMethod);
                            return false;
                        }

                        cachedEntry = new UserEntry(username, hashAlg, hashValue, statusValue, failedAttempts, blockedUntilUtc, false);

                        if (Settings.IsUserStatusValidationEnabled() &&
                            !string.Equals(cachedEntry.StatusValue, Settings.GetUserActiveValue(), StringComparison.OrdinalIgnoreCase))
                        {
                            m_logger.WarnFormat(
                                "Offline cache rejected user {0} because cached status '{1}' does not match '{2}'",
                                username,
                                cachedEntry.StatusValue,
                                Settings.GetUserActiveValue());
                            return false;
                        }

                        if (cachedEntry.IsCurrentlyLocked())
                        {
                            m_logger.WarnFormat(
                                "Offline cache rejected user {0} because account is locked until {1:u}",
                                username,
                                cachedEntry.BlockedUntilUtc.Value);
                            return false;
                        }

                        reader.Close();
                        if (cachedEntry.VerifyPassword(password))
                        {
                            cmd.CommandText = "UPDATE users SET failed_attempts = 0, blocked_until_utc = NULL WHERE username = @username";
                            cmd.ExecuteNonQuery();
                            return true;
                        }
                        cmd.CommandText = "UPDATE users SET failed_attempts = failed_attempts + 1, blocked_until_utc = CASE WHEN failed_attempts + 1 >= @maximum THEN @until ELSE blocked_until_utc END WHERE username = @username";
                        cmd.Parameters.AddWithValue("@maximum", Settings.GetMaxFailedAttempts());
                        cmd.Parameters.AddWithValue("@until", DateTime.UtcNow.AddMinutes(Settings.GetLockoutMinutes()).ToString("o", CultureInfo.InvariantCulture));
                        cmd.ExecuteNonQuery();
                        return false;
                    }
                }
            }
        }

        public static DateTime? GetLastSyncUtc()
        {
            lock (m_syncRoot)
            {
                Initialize();

                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT value FROM metadata WHERE key = 'last_sync_utc'";
                    object value = cmd.ExecuteScalar();
                    DateTime parsed;

                    if (value != null &&
                        value != DBNull.Value &&
                        DateTime.TryParse(
                            Convert.ToString(value),
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.RoundtripKind,
                            out parsed))
                    {
                        return parsed;
                    }
                }
            }

            return null;
        }

        public static void SetLastSyncUtc(DateTime value)
        {
            lock (m_syncRoot)
            {
                Initialize();

                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "INSERT INTO metadata (key, value) VALUES ('last_sync_utc', @value) " +
                        "ON CONFLICT(key) DO UPDATE SET value = excluded.value;";
                    cmd.Parameters.AddWithValue("@value", value.ToString("o", CultureInfo.InvariantCulture));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static string TestConfiguration()
        {
            var sb = new StringBuilder();
            string dbPath = Settings.GetLocalCachePath();
            string nativeDirectory = SQLiteNativeBootstrap.GetNativeDirectory();
            string nativeDllPath = SQLiteNativeBootstrap.GetNativeDllPath();

            sb.AppendLine("Offline cache");
            sb.AppendLine("-------------------------------");
            sb.AppendLine(string.Format("Process architecture: {0}", Environment.Is64BitProcess ? "x64" : "x86"));
            sb.AppendLine(string.Format("Native SQLite dir: {0}", nativeDirectory));
            sb.AppendLine(string.Format("Native SQLite dll: {0}", File.Exists(nativeDllPath) ? nativeDllPath : "MISSING"));
            sb.AppendLine(string.Format("Cache file: {0}", dbPath));

            try
            {
                Initialize();
                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT 1";
                    cmd.ExecuteScalar();
                }

                sb.AppendLine("SQLite offline cache: OK");
                var status = GetStatus();
                sb.AppendLine(status.ToDisplayText());
            }
            catch (Exception ex)
            {
                sb.AppendLine(string.Format("SQLite offline cache ERROR: {0}", ex.Message));
            }

            return sb.ToString();
        }

        private static SQLiteConnection OpenConnection()
        {
            SQLiteNativeBootstrap.EnsureInitialized();
            var conn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", Settings.GetLocalCachePath()));
            conn.Open();
            return conn;
        }

        public static OfflineCacheStatus GetStatus()
        {
            lock (m_syncRoot)
            {
                if (!File.Exists(Settings.GetLocalCachePath())) return new OfflineCacheStatus();
                Initialize();
                using (var conn = OpenConnection())
                {
                    var status = new OfflineCacheStatus { Exists = true };
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*), COALESCE(SUM(CASE WHEN julianday(updated_utc) >= julianday(@cutoff) AND julianday(updated_utc) <= julianday(@future) THEN 1 ELSE 0 END), 0) FROM users";
                        cmd.Parameters.AddWithValue("@cutoff", DateTime.UtcNow.AddHours(-Settings.GetOfflineCacheMaxAgeHours()).ToString("o", CultureInfo.InvariantCulture));
                        cmd.Parameters.AddWithValue("@future", DateTime.UtcNow.AddMinutes(2).ToString("o", CultureInfo.InvariantCulture));
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            status.UserCount = Convert.ToInt32(reader[0]);
                            status.FreshUserCount = Convert.ToInt32(reader[1]);
                        }
                    }
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT key, value FROM metadata WHERE key IN ('last_sync_utc','last_attempt_utc','last_attempt_result')";
                        using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                        {
                            var key = Convert.ToString(reader[0]);
                            if (key == "last_sync_utc") status.LastFullSyncUtc = ParseUtc(reader[1]);
                            else if (key == "last_attempt_utc") status.LastAttemptUtc = ParseUtc(reader[1]);
                            else status.LastAttemptResult = Convert.ToString(reader[1]);
                        }
                    }
                    return status;
                }
            }
        }

        internal static void RecordSyncAttempt(string result)
        {
            lock (m_syncRoot)
            {
                Initialize();
                using (var conn = OpenConnection())
                using (var transaction = conn.BeginTransaction())
                {
                    WriteMetadata(conn, transaction, "last_attempt_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                    WriteMetadata(conn, transaction, "last_attempt_result", result);
                    transaction.Commit();
                }
            }
        }

        private static void WriteMetadata(SQLiteConnection connection, SQLiteTransaction transaction, string key, string value)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = "INSERT INTO metadata (key,value) VALUES (@key,@value) ON CONFLICT(key) DO UPDATE SET value=excluded.value";
                cmd.Parameters.AddWithValue("@key", key);
                cmd.Parameters.AddWithValue("@value", value);
                cmd.ExecuteNonQuery();
            }
        }

        private static string ProtectHash(string value)
        {
            return "dpapi:" + Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(value), null, DataProtectionScope.LocalMachine));
        }

        private static void EnsureOptionalColumns()
        {
            TryAddColumn("ALTER TABLE users ADD COLUMN failed_attempts INTEGER NOT NULL DEFAULT 0");
            TryAddColumn("ALTER TABLE users ADD COLUMN blocked_until_utc TEXT NULL");
        }

        private static void TryAddColumn(string sql)
        {
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SQLiteException)
            {
                // Column already exists.
            }
        }

        private static bool TryParseHashAlgorithm(string hashMethod, out PasswordHashAlgorithm hashAlg)
        {
            hashAlg = PasswordHashAlgorithm.NONE;

            if (string.IsNullOrWhiteSpace(hashMethod))
                return true;

            return Enum.TryParse(hashMethod.Trim(), true, out hashAlg);
        }

        private static DateTime? ParseUtc(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            DateTime parsed;
            if (DateTime.TryParse(
                Convert.ToString(value),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out parsed))
            {
                return parsed.Kind == DateTimeKind.Utc ? parsed : parsed.ToUniversalTime();
            }

            return null;
        }
    }
}

