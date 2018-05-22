using System;
using System.Text;

namespace TAI_Forum.Infrastructure
{
    public static class Extensions
    {
        public static string ToHexString(this byte[] array)
        {
            StringBuilder s = new StringBuilder();
            foreach (byte b in array)
                s.AppendFormat("{0:x2}", b);
            return s.ToString();
        }

        public static string ToDbString(this DateTime timestamp)
        {
            return timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public static DateTime ToDate(this string sDate)
        {
            return DateTime.ParseExact(sDate, "yyyy-MM-dd HH:mm:ss.fff", null);
        }
    }
}