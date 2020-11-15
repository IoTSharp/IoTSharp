using System;

namespace AntDesign.Pro.Template
{
    public static class DateTimeExtension
    {
        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;
        private const int Month = 30 * Day;

        // todo: Need to be localized
        public static string ToFriendlyDisplay(this DateTime dateTime)
        {
            var ts = DateTime.Now - dateTime;
            var delta = ts.TotalSeconds;
            if (delta < 0)
            {
                return "not yet";
            }
            if (delta < 1 * Minute)
            {
                return ts.Seconds == 1 ? "1秒前" : ts.Seconds + "秒前";
            }
            if (delta < 2 * Minute)
            {
                return "1分钟之前";
            }
            if (delta < 45 * Minute)
            {
                return ts.Minutes + "分钟";
            }
            if (delta < 90 * Minute)
            {
                return "1小时前";
            }
            if (delta < 24 * Hour)
            {
                return ts.Hours + "小时前";
            }
            if (delta < 48 * Hour)
            {
                return "昨天";
            }
            if (delta < 30 * Day)
            {
                return ts.Days + " 天之前";
            }
            if (delta < 12 * Month)
            {
                var months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "一个月之前" : months + "月之前";
            }
            else
            {
                var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "一年前" : years + "年前";
            }
        }
    }
}