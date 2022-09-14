using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace IoTSharp.Extensions
{
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class BitSectionAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly short _len;

        // This is a positional argument
        public BitSectionAttribute(short len)
        {
            _len = len;
        }

        public short Len
        {
            get { return _len; }
        }
        public int Index { get; set; }
        internal BitVector32.Section Section { get; set; }
    }
    public static class BitVector32Extensions
    {

        /// <summary>
        /// 取高位值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int BitHeightValue(this int value)
        {
            BitVector32 vector32 = new BitVector32(value);
            BitVector32.Section _58_height = BitVector32.CreateSection(0xf, BitVector32.CreateSection(0xf));
            return vector32[_58_height];

        }
        /// <summary>
        /// 去低位值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int BitLowValue(this int value)
        {
            BitVector32 vector32 = new BitVector32(value);
            BitVector32.Section _04_low = BitVector32.CreateSection(0xf);
            return vector32[_04_low];
        }

        public static Dictionary<string, int> ToDictionary(this BitVector32 vector32, Dictionary<string, short> secs)
        {
            Dictionary<string, int> warn = new();
            List<(string pi, BitSectionAttribute bsa)> pairs = new List<(string pi, BitSectionAttribute bsa)>();
            secs.Keys.ToList().ForEach(p =>
            {
                var bs = new BitSectionAttribute(secs[p]);
                if (bs != null)
                {
                    pairs.Add((p, bs));
                }
            });
            var lst = pairs.ToList();
            for (int i = 0; i < lst.Count; i++)
            {
                var bsa = lst[i].bsa;
                var pi = lst[i].pi;
                if (i == 0)
                {
                    bsa.Section = BitVector32.CreateSection((short)(Math.Pow(2, bsa.Len) - bsa.Len));

                }
                else
                {
                    bsa.Section = BitVector32.CreateSection((short)(Math.Pow(2, bsa.Len) - bsa.Len), lst[i - 1].bsa.Section);
                }
                warn.Add(pi, vector32[bsa.Section]);
            }
            return warn;
        }
        public static T? To<T>(this BitVector32 vector32) where T : class, new()
        {
            T? warn = new();
            List<(PropertyInfo pi, BitSectionAttribute bsa)> pairs = new List<(PropertyInfo pi, BitSectionAttribute bsa)>();
            warn?.GetType().GetProperties().ToList().ForEach(p =>
            {
                var bs = p.GetCustomAttributes(typeof(BitSectionAttribute), true).ToList().FirstOrDefault() as BitSectionAttribute;
                if (bs != null)
                {
                    pairs.Add((p, bs));
                }
            });
            var lst = pairs.OrderBy(k => k.bsa.Index).ToList();
            for (int i = 0; i < lst.Count; i++)
            {
                var bsa = lst[i].bsa;
                var pi = lst[i].pi;
                if (i == 0)
                {
                    bsa.Section = BitVector32.CreateSection((short)(2 ^ bsa.Len - 1));
                }
                else
                {
                    bsa.Section = BitVector32.CreateSection((short)(2 ^ bsa.Len - 1), lst[i - 1].bsa.Section);
                }
                pi.SetValue(warn, vector32[bsa.Section]);
            }
            return warn;
        }
    }
}
