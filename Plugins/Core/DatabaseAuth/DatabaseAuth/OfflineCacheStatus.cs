using System;
using System.Globalization;

namespace OpenCredential.Plugin.DatabaseAuth
{
    internal sealed class OfflineCacheStatus
    {
        public bool Exists { get; set; }
        public int UserCount { get; set; }
        public int FreshUserCount { get; set; }
        public DateTime? LastFullSyncUtc { get; set; }
        public DateTime? LastAttemptUtc { get; set; }
        public string LastAttemptResult { get; set; }

        public string ToDisplayText()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "Local copy: {0}\r\nCached users: {1}; records within the age limit: {2}\r\nLast complete synchronization: {3}\r\nLast attempt: {4} ({5})\r\nRecord freshness does not override account status, lockout, password or authorization rules.",
                Exists ? "available" : "not created", UserCount, FreshUserCount, Format(LastFullSyncUtc),
                Format(LastAttemptUtc), LastAttemptResult ?? "no recorded attempt");
        }

        private static string Format(DateTime? value)
        {
            return value.HasValue ? value.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture) : "not recorded";
        }
    }
}
