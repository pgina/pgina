using System;
using System.Data.SQLite;
using System.IO;
using OpenCredential.Plugin.DatabaseAuth;

internal static class Program
{
    private static int Main()
    {
        try
        {
            TestDatabaseUtcTime();
            var hash = BCryptHasher.HashPassword("Offline test password 123!");
            var user = new UserEntry("offline-test", PasswordHashAlgorithm.BCRYPT, hash, "1");
            LocalUserCache.UpsertUser(user);
            UserEntry cached;
            Assert(LocalUserCache.TryAuthenticate(user.Name, "Offline test password 123!", out cached), "Autenticacion offline valida.");
            using (var connection = Open())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT hash_value FROM users";
                Assert(Convert.ToString(command.ExecuteScalar()).StartsWith("dpapi:"), "Hash debe almacenarse protegido por DPAPI.");
            }
            for (var attempt = 0; attempt < Settings.GetMaxFailedAttempts(); attempt++)
                Assert(!LocalUserCache.TryAuthenticate(user.Name, "wrong", out cached), "Clave incorrecta debe rechazarse.");
            Assert(!LocalUserCache.TryAuthenticate(user.Name, "Offline test password 123!", out cached), "Bloqueo offline debe aplicarse.");
            LocalUserCache.UpsertUser(user);
            using (var connection = Open())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE users SET updated_utc = @date";
                command.Parameters.AddWithValue("@date", DateTime.UtcNow.AddDays(-2).ToString("o"));
                command.ExecuteNonQuery();
            }
            Assert(!LocalUserCache.TryAuthenticate(user.Name, "Offline test password 123!", out cached), "Cache vencida debe rechazarse.");
            LocalUserCache.UpsertUser(new UserEntry(user.Name, PasswordHashAlgorithm.BCRYPT, hash, "0"));
            Assert(!LocalUserCache.TryAuthenticate(user.Name, "Offline test password 123!", out cached), "Cuenta desactivada debe rechazarse.");
            LocalUserCache.ReplaceUsers(new[] { user });
            Assert(LocalUserCache.TryAuthenticate(user.Name, "Offline test password 123!", out cached), "Sincronizacion restaura cuenta vigente.");
            LocalUserCache.ReplaceUsers(new UserEntry[0]);
            Assert(!LocalUserCache.TryAuthenticate(user.Name, "Offline test password 123!", out cached), "Sincronizacion elimina usuarios revocados.");
            Console.WriteLine("PASS Desktop: SQLite, DPAPI, claves, bloqueo, caducidad, estado y revocacion.");
            return 0;
        }
        catch (Exception exception) { Console.WriteLine("FAIL Desktop: " + exception.GetType().Name + ": " + exception.Message); return 1; }
    }
    private static void TestDatabaseUtcTime()
    {
        var utc = new DateTime(2026, 10, 20, 13, 39, 35, DateTimeKind.Utc);
        var withoutZone = DateTime.SpecifyKind(utc, DateTimeKind.Unspecified);
        Assert(DatabaseUtcTime.Read(withoutZone) == utc, "SQL timestamp sin zona debe conservar ticks UTC, sin sumar el offset local.");
        Assert(DatabaseUtcTime.Read(withoutZone).Value.Kind == DateTimeKind.Utc, "SQL timestamp debe identificarse como UTC.");
        Assert(DatabaseUtcTime.Read(utc) == utc, "UTC no debe convertirse dos veces.");
        Assert(DatabaseUtcTime.Read(utc.ToLocalTime()) == utc, "DateTime Local explicito debe convertirse a UTC.");
        Assert(DatabaseUtcTime.Read("2026-10-20 13:39:35") == utc, "Texto SQL sin zona representa UTC.");
        Assert(DatabaseUtcTime.Read("2026-10-20T08:39:35-05:00") == utc, "Texto con offset explicito conserva el instante.");
        Assert(DatabaseUtcTime.Read("2026-10-20T13:39:35Z") == utc, "Texto UTC conserva el instante.");
        Assert(DatabaseUtcTime.Read(new DateTimeOffset(2026, 10, 20, 8, 39, 35, TimeSpan.FromHours(-5))) == utc, "DateTimeOffset conserva el instante UTC.");
        Assert(DatabaseUtcTime.Format(withoutZone) == "2026-10-20 13:39:35 UTC", "Mensaje de bloqueo debe identificar explicitamente UTC.");
        Assert(!DatabaseUtcTime.Read(null).HasValue && !DatabaseUtcTime.Read(DBNull.Value).HasValue, "Fechas nulas no crean bloqueo.");
        Assert(!DatabaseUtcTime.Read("not-a-date").HasValue, "Fecha invalida no se interpreta como una fecha valida.");
        var expired = DatabaseUtcTime.Read(DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(-2), DateTimeKind.Unspecified));
        Assert(!new UserEntry("expired", PasswordHashAlgorithm.NONE, "", "1", 3, expired, false).IsCurrentlyLocked(), "Bloqueo vencido en SQL no debe prolongarse por la zona del cliente.");
        var future = DatabaseUtcTime.Read(DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(2), DateTimeKind.Unspecified));
        Assert(new UserEntry("future", PasswordHashAlgorithm.NONE, "", "1", 3, future, false).IsCurrentlyLocked(), "Bloqueo vigente debe mantenerse.");
        Assert(new UserEntry("server-locked", PasswordHashAlgorithm.NONE, "", "1", 3, expired, true).IsCurrentlyLocked(), "No ignorar un bloqueo vigente informado por el servidor.");
        Console.WriteLine("PASS Desktop: lectura UTC y vencimiento de bloqueo sin desplazamiento local.");
    }
    private static SQLiteConnection Open() { var connection = new SQLiteConnection("Data Source=" + Settings.GetLocalCachePath() + ";Version=3;"); connection.Open(); return connection; }
    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
}

namespace OpenCredential.Plugin.DatabaseAuth
{
    // Configuration is the boundary under test; never access the real Windows registry.
    internal static class Settings
    {
        private static readonly string Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "oc-desktop-test-" + Guid.NewGuid().ToString("N"), "cache.sqlite");
        public enum HashEncoding { HEX, BASE_64 }
        public static HashEncoding GetHashEncoding() { return HashEncoding.HEX; }
        public static string GetLocalCachePath() { return Path; }
        public static bool IsUserStatusValidationEnabled() { return true; }
        public static string GetUserActiveValue() { return "1"; }
        public static int GetOfflineCacheMaxAgeHours() { return 24; }
        public static int GetMaxFailedAttempts() { return 3; }
        public static int GetLockoutMinutes() { return 15; }
    }
}
