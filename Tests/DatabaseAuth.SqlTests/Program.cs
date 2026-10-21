using System;
using System.Reflection;
using Npgsql;
using OpenCredential.Plugin.DatabaseAuth;

internal static class Program
{
    private static int Main()
    {
        try
        {
            using (var source = new PostgreSqlUserDataSource())
            {
                var connection = (NpgsqlConnection)typeof(PostgreSqlUserDataSource).GetField("m_conn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(source);
                string[] types = { "integer", "bigint", "uuid", "text" };
                string[] ids = { "7", "5000000000", "'12345678-1234-1234-1234-123456789abc'", "'user-7'" };
                for (int index = 0; index < types.Length; index++)
                {
                    Settings.Store.Table = "qa_membership_users_" + index;
                    Settings.Store.GroupTableName = "qa_membership_groups_" + index;
                    Settings.Store.UserGroupTableName = "qa_membership_maps_" + index;
                    string users = Settings.Store.Table, groups = Settings.Store.GroupTableName, maps = Settings.Store.UserGroupTableName;
                    using (var command = new NpgsqlCommand(
                        "CREATE TEMP TABLE " + users + " (id " + types[index] + ", username text); " +
                        "CREATE TEMP TABLE " + groups + " (group_id bigint, group_name text); " +
                        "CREATE TEMP TABLE " + maps + " (user_id " + types[index] + ", group_id bigint); " +
                        "INSERT INTO " + users + " VALUES (" + ids[index] + ", 'qa-student'), (NULL, 'qa-null-id'); " +
                        "INSERT INTO " + groups + " VALUES (1, 'RdpTestOnly'); " +
                        "INSERT INTO " + maps + " SELECT id, 1 FROM " + users + " WHERE id IS NOT NULL", connection)) command.ExecuteNonQuery();
                    Assert(source.IsMemberOfGroup("qa-student", "RdpTestOnly"));
                    Assert(source.IsMemberOfGroup("qa-student", "RDPTESTONLY"));
                    Assert(!source.IsMemberOfGroup("qa-student", "not-a-group"));
                    Assert(!source.IsMemberOfGroup("missing-user", "RdpTestOnly"));
                    Assert(!source.IsMemberOfGroup("qa-null-id", "RdpTestOnly"));
                    Assert(!source.IsMemberOfGroup("qa-student' OR '1'='1", "RdpTestOnly"));
                    Console.WriteLine("PASS PostgreSQL membership: " + types[index] + " ID, positive/negative/missing/null and parameterized lookup.");
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            while (ex is TypeInitializationException && ex.InnerException != null) ex = ex.InnerException;
            var pg = ex as PostgresException;
            Console.WriteLine("FAIL Membership: " + ex.GetType().Name + (pg == null ? "" : " SQLSTATE=" + pg.SqlState));
            return 1;
        }
    }
    private static void Assert(bool success) { if (!success) throw new InvalidOperationException("Synthetic membership assertion failed."); }
}

namespace OpenCredential.Plugin.DatabaseAuth
{
    // Isolated settings boundary: no registry, real plugin settings or user data.
    internal static class Settings
    {
        private static readonly NpgsqlConnectionStringBuilder Connection = new NpgsqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("OC_QA_GROUP_CONNECTION") ?? throw new InvalidOperationException("Explicit synthetic connection required."));
        public static readonly TestStore Store = new TestStore();
        public enum HashEncoding { HEX, BASE_64 }
        public static HashEncoding GetHashEncoding() { return HashEncoding.HEX; }
        public sealed class TestStore
        {
            public string Host { get { return Connection.Host; } }
            public string User { get { return Connection.Username; } }
            public string Database { get { return Connection.Database; } }
            public string GetEncryptedSetting(string name) { return Connection.Password; }
            public string Table = "qa_membership_users", GroupTableName = "qa_membership_groups", UserGroupTableName = "qa_membership_maps";
            public string UsernameColumn = "username", UserTablePrimaryKeyColumn = "id", HashMethodColumn = "hash_method", PasswordColumn = "password_hash";
            public string UserForeignKeyColumn = "user_id", GroupForeignKeyColumn = "group_id", GroupTablePrimaryKeyColumn = "group_id", GroupNameColumn = "group_name";
        }
        public static int GetPort() { return Connection.Port; }
        public static MySqlConnector.MySqlSslMode GetSslMode() { return MySqlConnector.MySqlSslMode.None; }
        public static int GetConnectionTimeout() { return 5; }
        public static int GetCommandTimeout() { return 15; }
        public static bool IsUserStatusValidationEnabled() { return true; }
        public static bool IsLoginLockoutEnabled() { return true; }
        public static string GetUserStatusColumn() { return "status"; }
        public static string GetUserActiveValue() { return "1"; }
        public static string GetFailedAttemptsColumn() { return "failed_attempts"; }
        public static string GetBlockedUntilColumn() { return "locked_until"; }
        public static string GetLastAttemptColumn() { return "last_attempt_at"; }
        public static int GetMaxFailedAttempts() { return 3; }
        public static int GetLockoutMinutes() { return 1; }
    }
}
