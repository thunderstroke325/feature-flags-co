using System;

namespace FeatureFlags.Utils.ExtensionMethods
{
    public static class DateTimeExtensions
    {
        public static long UnixTimestamp(this DateTime time)
        {
            var seconds = (long)time.Subtract(DateTime.UnixEpoch).TotalSeconds;

            return seconds;
        }
    }
}