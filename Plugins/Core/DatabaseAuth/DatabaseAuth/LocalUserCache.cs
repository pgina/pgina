using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;
using log4net;

namespace OpenCredential.Plugin.DatabaseAuth
{
    class LocalUserCache
    {
        private static readonly ILog m_logger = LogManager.GetLogger("DatabaseAuth.LocalUserCache");
        private static readonly object m_syncRoot = new object();

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
            if (entries == null)
                return;

            lock (m_syncRoot)
            {
                Initialize();

                using (var conn = OpenConnection())
                using (var transaction = conn.BeginTransaction())
                {
                    foreach (UserEntry entry in entries)
                    {
                        if (entry == null || string.IsNullOrWhiteSpace(entry.Name))
                            continue;

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            cmd.CommandText =
                                "INSERT INTO users (username, hash_method, hash_value, status_value, updated_utc) " +
                                "VALUES (@username, @hash_method, @hash_value, @status_value, @updated_utc) " +
                                "ON CONFLICT(username) DO UPDATE SET " +
                                "hash_method = excluded.hash_method, " +
                                "hash_value = excluded.hash_value, " +
                                "status_value = excluded.status_value, " +
                                "updated_utc = excluded.updated_utc;";
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
                            cmd.Parameters.AddWithValue("@hash_value", entry.Hash ?? string.Empty);
                            cmd.Parameters.AddWithValue("@status_value", (object)entry.StatusValue ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@failed_attempts", entry.FailedAttempts);
                            cmd.Parameters.AddWithValue(
                                "@blocked_until_utc",
                                entry.BlockedUntilUtc.HasValue
                                    ? (object)entry.BlockedUntilUtc.Value.ToString("o", CultureInfo.InvariantCulture)
                                    : DBNull.Value);
                            cmd.Parameters.AddWithValue("@updated_utc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
                            cmd.ExecuteNonQuery();
                        }
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
                    cmd.CommandText = "SELECT hash_method, hash_value, status_value, failed_attempts, blocked_until_utc FROM users WHERE username = @username";
                    cmd.Parameters.AddWithValue("@username", username);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        string hashMethod = Convert.ToString(reader["hash_method"]);
                        string hashValue = Convert.ToString(reader["hash_value"]);
                        string statusValue = reader["status_value"] == DBNull.Value
                            ? null
                            : Convert.ToString(reader["status_value"]);
                        int failedAttempts = reader["failed_attempts"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["failed_attempts"]);
                        DateTime? blockedUntilUtc = ParseUtc(reader["blocked_until_utc"]);
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

                        if (Settings.IsLoginLockoutEnabled() && cachedEntry.IsCurrentlyLocked())
                        {
                            m_logger.WarnFormat(
                                "Offline cache rejected user {0} because account is locked until {1:u}",
                                username,
                                cachedEntry.BlockedUntilUtc.Value);
                            return false;
                        }

                        return cachedEntry.VerifyPassword(password);
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

