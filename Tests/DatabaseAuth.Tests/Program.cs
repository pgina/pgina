using System;
using System.Data.SQLite;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using OpenCredential.Plugin.DatabaseAuth;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length > 0) return HoldSynchronization(args);
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
            TestCompleteSynchronization(hash);
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

    private static void TestCompleteSynchronization(string hash)
    {
        const string password = "Offline test password 123!";
        var firstVisit = new UserEntry("registered-first-visit", PasswordHashAlgorithm.BCRYPT, hash, "1");
        var inactive = new UserEntry("replicated-inactive", PasswordHashAlgorithm.BCRYPT, hash, "0");
        UserEntry cached;
        Assert(!LocalUserCache.TryAuthenticate(firstVisit.Name, password, out cached), "Usuario ausente no se inventa.");
        Assert(CacheSynchronization.Run(() => new[] { firstVisit, inactive }, TimeSpan.FromMinutes(5), true) == CacheSyncOutcome.Completed, "Descarga completa confirmada.");
        Assert(LocalUserCache.TryAuthenticate(firstVisit.Name, password, out cached), "Primer ingreso offline sin login previo en este equipo.");
        Assert(!LocalUserCache.TryAuthenticate(inactive.Name, password, out cached), "Sincronizar no reactiva usuarios inactivos.");
        var before = LocalUserCache.GetStatus();
        Assert(before.UserCount == 2 && before.FreshUserCount == 2 && before.LastFullSyncUtc.HasValue, "Estado con total, frescura y UTC.");
        Assert(!before.ToDisplayText().Contains(firstVisit.Name) && !before.ToDisplayText().Contains(hash), "Estado sin identidades ni hashes.");
        Assert(CacheSynchronization.Run(() => { throw new IOException("synthetic outage"); }, TimeSpan.Zero, true) == CacheSyncOutcome.Failed, "Fallo de descarga registrado.");
        Assert(LocalUserCache.GetStatus().LastFullSyncUtc == before.LastFullSyncUtc && LocalUserCache.GetStatus().LastAttemptResult == "failed", "Fallo no renueva la antiguedad del snapshot.");
        Assert(LocalUserCache.TryAuthenticate(firstVisit.Name, password, out cached), "Caida conserva la copia valida anterior.");
        Assert(CacheSynchronization.Run(() => IncompleteSnapshot(firstVisit), TimeSpan.Zero, true) == CacheSyncOutcome.Failed, "Enumeracion parcial rechazada.");
        Assert(LocalUserCache.GetStatus().UserCount == 2, "Respuesta parcial no elimina la copia anterior.");
        Assert(CacheSynchronization.Run(() => null, TimeSpan.Zero, true) == CacheSyncOutcome.Failed, "Null no equivale a base vacia.");
        Assert(CacheSynchronization.Run(() => new[] { firstVisit, firstVisit }, TimeSpan.Zero, true) == CacheSyncOutcome.Failed, "Identidades duplicadas no producen una copia ambigua.");
        using (var connection = Open())
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "CREATE TRIGGER abort_sync_metadata BEFORE INSERT ON metadata WHEN NEW.key='last_sync_utc' BEGIN SELECT RAISE(ABORT,'synthetic commit failure'); END";
            command.ExecuteNonQuery();
        }
        Assert(CacheSynchronization.Run(() => new[] { inactive }, TimeSpan.Zero, true) == CacheSyncOutcome.Failed, "Fallo al grabar timestamp aborta tambien el reemplazo de usuarios.");
        Assert(LocalUserCache.GetStatus().UserCount == 2 && LocalUserCache.GetStatus().LastFullSyncUtc == before.LastFullSyncUtc, "Usuarios y metadata se revierten juntos.");
        using (var connection = Open())
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "DROP TRIGGER abort_sync_metadata";
            command.ExecuteNonQuery();
        }
        int calls = 0;
        Assert(CacheSynchronization.Run(() => { calls++; return new[] { firstVisit }; }, TimeSpan.FromMinutes(5), false) == CacheSyncOutcome.NotDue && calls == 0, "No consultar antes del intervalo.");
        CacheSyncOutcome nested = CacheSyncOutcome.Failed;
        Assert(CacheSynchronization.Run(() =>
        {
            nested = CacheSynchronization.Run(() => new UserEntry[0], TimeSpan.Zero, true);
            return new[] { firstVisit };
        }, TimeSpan.Zero, true) == CacheSyncOutcome.Completed && nested == CacheSyncOutcome.Busy, "No solapar sincronizadores del proceso.");
        using (var connection = Open())
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "UPDATE users SET updated_utc=@future";
            command.Parameters.AddWithValue("@future", DateTime.UtcNow.AddDays(2).ToString("o"));
            command.ExecuteNonQuery();
        }
        Assert(!LocalUserCache.TryAuthenticate(firstVisit.Name, password, out cached) && LocalUserCache.GetStatus().FreshUserCount == 0, "Retroceso del reloj no prolonga registros del futuro.");
        Assert(CacheSynchronization.Run(() => new UserEntry[0], TimeSpan.Zero, true) == CacheSyncOutcome.Completed, "Snapshot vacio confirmado revoca usuarios, a diferencia de un error.");
        Assert(LocalUserCache.GetStatus().UserCount == 0 && !LocalUserCache.TryAuthenticate(firstVisit.Name, password, out cached), "Revocacion efectiva tras sincronizacion vacia.");
        TestCrossProcessSynchronization();
        Console.WriteLine("PASS Desktop: replica completa, primer ingreso offline, copia anterior ante fallos, metadata atomica, exclusion mutua y estado sin credenciales.");
    }

    private static IEnumerable<UserEntry> IncompleteSnapshot(UserEntry entry)
    {
        yield return entry;
        throw new IOException("synthetic interrupted response");
    }

    private static int HoldSynchronization(string[] args)
    {
        if (args.Length != 2 || args[0] != "--hold-sync" ||
            !System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(args[1])).StartsWith("oc-desktop-test-", StringComparison.Ordinal) ||
            System.IO.Path.GetFileName(args[1]) != "cache.sqlite")
            throw new ArgumentException("Only a synthetic test-cache path is accepted.");
        Settings.CachePathOverride = args[1];
        var directory = System.IO.Path.GetDirectoryName(args[1]);
        var result = CacheSynchronization.Run(() =>
        {
            File.WriteAllText(System.IO.Path.Combine(directory, "sync-ready"), "ready");
            var limit = Stopwatch.StartNew();
            while (!File.Exists(System.IO.Path.Combine(directory, "sync-release")))
            {
                if (limit.Elapsed > TimeSpan.FromSeconds(15)) throw new TimeoutException("Synthetic lock holder timed out.");
                Thread.Sleep(20);
            }
            return new UserEntry[0];
        }, TimeSpan.Zero, true);
        return result == CacheSyncOutcome.Completed ? 0 : 1;
    }

    private static void TestCrossProcessSynchronization()
    {
        var directory = System.IO.Path.GetDirectoryName(Settings.GetLocalCachePath());
        var start = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location,
            "--hold-sync \"" + Settings.GetLocalCachePath() + "\"") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        using (var child = Process.Start(start))
        {
            var output = child.StandardOutput.ReadToEndAsync();
            var error = child.StandardError.ReadToEndAsync();
            try
            {
                var wait = Stopwatch.StartNew();
                while (!File.Exists(System.IO.Path.Combine(directory, "sync-ready")) && !child.HasExited && wait.Elapsed < TimeSpan.FromSeconds(10)) Thread.Sleep(20);
                Assert(File.Exists(System.IO.Path.Combine(directory, "sync-ready")), "El proceso aislado obtiene el mutex compartido.");
                Assert(CacheSynchronization.Run(() => { throw new Exception("must not fetch"); }, TimeSpan.Zero, true) == CacheSyncOutcome.Busy,
                    "Servicio e interfaz no deben sincronizar el mismo archivo simultaneamente.");
            }
            finally
            {
                File.WriteAllText(System.IO.Path.Combine(directory, "sync-release"), "release");
                if (!child.WaitForExit(10000)) { child.Kill(); child.WaitForExit(); }
            }
            output.Wait(); error.Wait();
            Assert(child.ExitCode == 0, "La sincronizacion del segundo proceso termina sin errores.");
        }
    }
    private static SQLiteConnection Open() { var connection = new SQLiteConnection("Data Source=" + Settings.GetLocalCachePath() + ";Version=3;"); connection.Open(); return connection; }
    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
}

namespace OpenCredential.Plugin.DatabaseAuth
{
    // Configuration is the boundary under test; never access the real Windows registry.
    internal static class Settings
    {
        private static readonly string Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "opencode", "oc-desktop-test-" + Guid.NewGuid().ToString("N"), "cache.sqlite");
        internal static string CachePathOverride;
        public enum HashEncoding { HEX, BASE_64 }
        public static HashEncoding GetHashEncoding() { return HashEncoding.HEX; }
        public static string GetLocalCachePath() { return CachePathOverride ?? Path; }
        public static bool IsUserStatusValidationEnabled() { return true; }
        public static string GetUserActiveValue() { return "1"; }
        public static int GetOfflineCacheMaxAgeHours() { return 24; }
        public static int GetMaxFailedAttempts() { return 3; }
        public static int GetLockoutMinutes() { return 15; }
    }
}
