using System;

namespace CsharpExtensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Get max date between two dates
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static DateTime Max(DateTime date1, DateTime date2)
        {
            return date1 > date2 ? date1 : date2;
        }

        /// <summary>
        /// Get min date between two dates
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns></returns>
        public static DateTime Min(DateTime date1, DateTime date2)
        {
            return date1 < date2 ? date1 : date2;
        }

        public static string ToHumanReadableString(this TimeSpan t)
        {
            if (t.TotalSeconds <= 1) { return $@"{t:s\.ff} seconds"; }
            if (t.TotalMinutes <= 1) { return $@"{t.TotalSeconds} seconds"; }
            if (t.TotalHours <= 1) { return $@"{t.Minutes}:{t.Seconds} minutes"; }
            if (t.TotalDays <= 1) { return $@"{t.Hours}:{t.Minutes} hours"; }
            return $@"{t.Days} days and {t.Hours}:{t.Minutes} hours";
        }

        public static DateTime StartOfDay(this DateTime date) => date.Date;
        public static DateTime EndOfDay(this DateTime date) => date.StartOfDay().AddDays(1).AddTicks(-1);
        public static DateTime? FixUnixDate(this DateTime date) => date < new DateTime(1970, 1, 1) ? null : (DateTime?)date;
        public static long ToUnix(this DateTime date) => date < new DateTime(1970, 1, 1) ? 0 : ((DateTimeOffset)date).ToUnixTimeSeconds();
        public static string MongoISO(this DateTime date) => date.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ");
    }
}
