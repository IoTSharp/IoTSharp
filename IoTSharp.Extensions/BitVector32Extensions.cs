using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

namespace IoTSharp.Extensions
{


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
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
        public static T To<T>(this BitVector32 vector32) where T : class, new()
        {
            T warn = new();
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
                    bsa.Section = BitVector32.CreateSection(bsa.Len);
                }
                else
                {
                    bsa.Section = BitVector32.CreateSection(bsa.Len, lst[i - 1].bsa.Section);
                }
                pi.SetValue(warn, vector32[bsa.Section]);
            }
            return warn;
        }
    }
}
