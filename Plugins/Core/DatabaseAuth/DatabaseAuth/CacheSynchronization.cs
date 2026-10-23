using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using log4net;

namespace OpenCredential.Plugin.DatabaseAuth
{
    internal enum CacheSyncOutcome { Completed, NotDue, Busy, Disabled, Failed }

    internal static class CacheSynchronization
    {
        private static int running;
        private static readonly ILog Logger = LogManager.GetLogger("DatabaseAuth.CacheSynchronization");

        // The named mutex coordinates the service and configuration process, including different sessions.
        internal static CacheSyncOutcome Run(Func<IEnumerable<UserEntry>> fetch, TimeSpan interval, bool force)
        {
            if (fetch == null) throw new ArgumentNullException("fetch");
            if (Interlocked.CompareExchange(ref running, 1, 0) != 0) return CacheSyncOutcome.Busy;
            try
            {
                var security = new MutexSecurity();
                foreach (var sid in new[] { WindowsIdentity.GetCurrent().User,
                    new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                    new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null) })
                    security.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
                string name;
                using (var sha = SHA256.Create())
                    name = "Global\\OpenCredential.CacheSync." + BitConverter.ToString(sha.ComputeHash(
                        Encoding.UTF8.GetBytes(Path.GetFullPath(Settings.GetLocalCachePath()).ToUpperInvariant()))).Replace("-", "");
                bool created;
                using (var mutex = new Mutex(false, name, out created, security))
                {
                    bool acquired;
                    try { acquired = mutex.WaitOne(0); }
                    catch (AbandonedMutexException) { acquired = true; }
                    if (!acquired) return CacheSyncOutcome.Busy;
                    try
                    {
                        var last = LocalUserCache.GetLastSyncUtc();
                        if (!force && last.HasValue && last.Value <= DateTime.UtcNow && DateTime.UtcNow - last.Value < interval)
                            return CacheSyncOutcome.NotDue;
                        LocalUserCache.RecordSyncAttempt("in_progress");
                        var users = fetch();
                        // Timestamp and complete replacement commit in the same SQLite transaction.
                        LocalUserCache.ReplaceUsers(users);
                        Logger.Info("Complete local user synchronization committed.");
                        return CacheSyncOutcome.Completed;
                    }
                    finally { mutex.ReleaseMutex(); }
                }
            }
            catch (Exception error)
            {
                // Never expose connection strings, hashes or provider error messages in the status UI.
                Logger.WarnFormat("User synchronization failed ({0}); the previous complete copy is preserved.", error.GetType().Name);
                try { LocalUserCache.RecordSyncAttempt("failed"); }
                catch (Exception) { Logger.Warn("Unable to record synchronization status."); }
                return CacheSyncOutcome.Failed;
            }
            finally { Interlocked.Exchange(ref running, 0); }
        }
    }
}
