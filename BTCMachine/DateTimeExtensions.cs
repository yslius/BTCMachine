using System;

namespace BTCMachine
{
    internal static class DateTimeExtensions
    {
        private static readonly DateTimeOffset UnixEpochDateTimeOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTime UnixEpochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static long ToUnixTime(this DateTimeOffset datetime) => (long)datetime.ToUniversalTime().Subtract(DateTimeExtensions.UnixEpochDateTimeOffset).TotalSeconds;

        internal static long ToUnixTime(this DateTime datetime) => (long)datetime.ToUniversalTime().Subtract(DateTimeExtensions.UnixEpochDateTime).TotalSeconds;

        internal static long ToUnixTimeMilliseconds(this DateTimeOffset datetime) => (long)datetime.ToUniversalTime().Subtract(DateTimeExtensions.UnixEpochDateTimeOffset).TotalMilliseconds;

        internal static long ToUnixTimeMilliseconds(this DateTime datetime) => (long)datetime.ToUniversalTime().Subtract(DateTimeExtensions.UnixEpochDateTime).TotalMilliseconds;
    }
}
