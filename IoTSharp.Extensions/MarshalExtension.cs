using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Runtime.InteropServices
{
    public static class MarshalExtension
    {
        public static byte[] ToBytes<T>(this T t) where T : struct
        {
            int rawsize = Marshal.SizeOf<T>();//得到内存大小
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);//分配内存
            Marshal.StructureToPtr(t, buffer, true);//转换结构
            byte[] rawdatas = new byte[rawsize];
            Marshal.Copy(buffer, rawdatas, 0, rawsize);//拷贝内存
            Marshal.FreeHGlobal(buffer); //释放内存
            return rawdatas;
        }
        public static T ToStruct<T>(this byte[] rawdatas) where T : struct
        {
            var t = default(T);
            Type anytype = typeof(T);
            int rawsize = Marshal.SizeOf(anytype);
            if (rawsize > rawdatas.Length)
                t = default(T);
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(rawdatas, 0, buffer, rawsize);
            object retobj = Marshal.PtrToStructure(buffer, anytype);
            Marshal.FreeHGlobal(buffer);
            t = (T)retobj;
            return t;
        }
    }
}
