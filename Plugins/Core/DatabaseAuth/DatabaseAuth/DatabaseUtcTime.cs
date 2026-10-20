using System;
using System.Globalization;

namespace OpenCredential.Plugin.DatabaseAuth
{
    internal static class DatabaseUtcTime
    {
        public static DateTime? Read(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            // SQL sessions use UTC. TIMESTAMP/DATETIME columns carry no zone and
            // drivers return Kind=Unspecified: label those ticks, do not shift them.
            if (value is DateTime dateTime)
                return dateTime.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                    : dateTime.ToUniversalTime();

            if (value is DateTimeOffset offset)
                return offset.UtcDateTime;

            if (DateTime.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime parsed))
                return parsed;

            return null;
        }

        public static string Format(DateTime value)
        {
            return Read(value).Value.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
        }
    }
}
