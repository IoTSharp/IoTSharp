using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IoTSharp.Extensions
{
    public static class StringExtension
    {
        public static string ToTitleCase(this string str)
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }
        public static string Left(this string str, int length)
        {
            str = (str ?? string.Empty);
            return str.Substring(0, Math.Min(length, str.Length));
        }

        public static string Right(this string str, int length)
        {
            str = (str ?? string.Empty);
            return (str.Length >= length)
                ? str.Substring(str.Length - length, length)
                : str;
        }

        public static char[] Right(this char[] str, int length)
        {
            return str.Skip(str.Length - length).Take(length).ToArray();
        }
        public static char[] Left(this char[] str, int length)
        {
            return str.Take(length).ToArray();
        }



        public static byte[] Right(this byte[] str, int length)
        {
            return str.Skip(str.Length - length).Take(length).ToArray();
        }
        public static byte[] Left(this byte[] str, int length)
        {
            return str.Take(length).ToArray();
        }

    }
}
