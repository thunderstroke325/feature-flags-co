using System;

namespace FeatureFlags.Utils.ExtensionMethods
{
    public static class LongExtensions
    {
        public static DateTime UnixTimestampInMillisecondsToDateTime(this long timestamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
        }

        public static DateTime UnixTimestampInSecondsToDateTime(this long timestamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
        }
    }
}
