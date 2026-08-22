using System;

namespace OpenCredential.Plugin.DatabaseAuth
{
    class FailedAttemptResult
    {
        public int FailedAttempts { get; set; }
        public DateTime? BlockedUntilUtc { get; set; }
        public bool IsLockedByDatabase { get; set; }

        public bool IsLocked
        {
            get { return IsLockedByDatabase || (BlockedUntilUtc.HasValue && BlockedUntilUtc.Value > DateTime.UtcNow); }
        }
    }
}

