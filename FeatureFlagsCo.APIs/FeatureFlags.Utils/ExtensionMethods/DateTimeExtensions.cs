using System;

namespace FeatureFlags.Utils.ExtensionMethods
{
    public static class DateTimeExtensions
    {
        public static long UnixTimestampInMilliseconds(this DateTime dateTime)
        {
            var milliseconds = (long)dateTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds;

            return milliseconds;
        }

        public static long UnixTimestampInSeconds(this DateTime time)
        {
            var seconds = (long)time.Subtract(DateTime.UnixEpoch).TotalSeconds;

            return seconds;
        }
    }
}