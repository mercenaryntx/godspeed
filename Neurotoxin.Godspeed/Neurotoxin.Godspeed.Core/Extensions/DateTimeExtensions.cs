using System;
using System.Diagnostics;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static int ToFatFileTime(this DateTime time)
        {
            var y = 0;
            y |= ((time.Year - 1980) & 0xEF) << 25;
            y |= (time.Month & 0xF) << 21;
            y |= (time.Day & 0x1F) << 16;
            y |= (time.Hour & 0x1F) << 11;
            y |= (time.Minute & 0x3F) << 5;
            y |= time.Second & 0x1F;
            return y;
        }

        public static double ToUnixTimestamp(this DateTime time)
        {
            return Math.Floor((time - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds);
        }

        public static DateTime FromFatFileTime(int time)
        {
            var year = (int)((time & 0xFE000000) >> 25) + 1980;
            var month = (time & 0x1E00000) >> 21;
            var day = (time & 0x1F0000) >> 16;
            var hour = (time & 0xF800) >> 11;
            var minute = (time & 0x7E0) >> 5;
            var second = time & 0x1F;
            try
            {
                return new DateTime(year, month, day, hour, minute, second);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}