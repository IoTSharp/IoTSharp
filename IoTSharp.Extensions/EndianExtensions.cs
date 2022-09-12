using System;
using System.Runtime.InteropServices;

namespace IoTSharp.Extensions
{
    public static class EndianExtensions
    {
        public static byte[] ToBytes(this short v) => BitConverter.GetBytes(v);
        public static short ToShort(this byte[] v) => BitConverter.ToInt16(v, 0);
        public static ushort ToUShort(this byte[] v) => BitConverter.ToUInt16(v, 0);
        public static byte[] ToBytes(this ushort v) => BitConverter.GetBytes(v);
        public static byte[] ToBytes(this int v) => BitConverter.GetBytes(v);
        public static int ToInt(this byte[] v) => BitConverter.ToInt32(v, 0);
        public static byte[] ToBytes(this uint v) => BitConverter.GetBytes(v);
        public static uint ToUInt(this byte[] v) => BitConverter.ToUInt32(v, 0);
        public static byte[] ToBytes(this long v) => BitConverter.GetBytes(v);
        public static ulong ToULong(this byte[] v) => BitConverter.ToUInt64(v, 0);
        public static byte[] ToBytes(this ulong v) => BitConverter.GetBytes(v);
        public static long ToLong(this byte[] v) => BitConverter.ToInt64(v, 0);
        public static int ToInt(this short v) => v;
        public static ushort ToUShort(this short v) => (ushort)v;

        public static uint ToUInt(this short v) => (uint)v;
        public static long ToLong(this short v) => v;
        public static ulong ToULong(this short v) => (ulong)v;

        public static int ToInt(this ushort v) => v;
        public static short ToShort(this short v) => v;
        public static uint ToUInt(this ushort v) => v;
        public static long ToLong(this ushort v) => v;
        public static ulong ToULong(this ushort v) => v;

        public static ushort ToUShort(this int v) => (ushort)v;
        public static short ToShort(this int v) => (short)v;
        public static uint ToUInt(this int v) => (uint)v;
        public static long ToLong(this int v) => v;
        public static ulong ToULong(this int v) => (ulong)v;



        public static ushort ToUShort(this uint v) => (ushort)v;
        public static short ToShort(this uint v) => (short)v;
        public static int ToInt(this uint v) => (int)v;
        public static long ToLong(this uint v) => v;
        public static ulong ToULong(this uint v) => v;


        public static ushort ToUShort(this long v) => (ushort)v;
        public static short ToShort(this long v) => (short)v;
        public static int ToInt(this long v) => (int)v;
        public static uint ToUInt(this long v) => (uint)v;
        public static ulong ToULong(this long v) => (ulong)v;



        public static ushort ToUShort(this ulong v) => (ushort)v;
        public static short ToShort(this ulong v) => (short)v;
        public static int ToInt(this ulong v) => (int)v;
        public static uint ToUInt(this ulong v) => (uint)v;
        public static long ToLong(this ulong v) => (long)v;


        public static short Swap(this short v)
        {
            return (short)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
        }
        public static ushort Swap(this ushort v)
        {
            return (ushort)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
        }
        public static int Swap(this int v)
        {
            return (int)(((Swap((short)v) & 0xffff) << 0x10) |
                          (Swap((short)(v >> 0x10)) & 0xffff));
        }
        public static uint Swap(this uint v)
        {
            return (uint)(((Swap((ushort)v) & 0xffff) << 0x10) |
                           (Swap((ushort)(v >> 0x10)) & 0xffff));
        }
        public static long Swap(this long v)
        {
            return (long)(((Swap((int)v) & 0xffffffffL) << 0x20) |
                           (Swap((int)(v >> 0x20)) & 0xffffffffL));
        }
        public static ulong Swap(this ulong v)
        {
            return (ulong)(((Swap((uint)v) & 0xffffffffL) << 0x20) |
                            (Swap((uint)(v >> 0x20)) & 0xffffffffL));
        }

        public static byte[] ToHexBytes(this string hexString)
        {
            hexString = hexString.Replace(" ", "");   //去除空格
            if ((hexString.Length % 2) != 0)     //判断hexstring的长度是否为偶数
            {
                hexString += "";
            }
            byte[] returnBytes = new byte[hexString.Length / 2];  //声明一个长度为hexstring长度一半的字节组returnBytes
            for (int i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);  //将hexstring的两个字符转换成16进制的字节组
            }
            return returnBytes;
        }

        //字节组转换成16进制的字符串：
        public static string ToHexStr(this byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");      //byte转16进制字符
                }
            }
            return returnStr;
        }

        static ushort[] crc_table = new ushort[] { 0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7, 0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF, 0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6, 0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE, 0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485, 0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D, 0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4, 0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC, 0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823, 0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B, 0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12, 0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A, 0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41, 0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49, 0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70, 0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78, 0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F, 0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067, 0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E, 0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256, 0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D, 0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405, 0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C, 0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634, 0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB, 0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3, 0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A, 0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92, 0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9, 0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1, 0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8, 0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0 };

        public static ushort ToCRC16(this byte[] buffer)
        {
            return ToShort16(buffer, (uint)buffer.Length, 0x00);
        }

        public static ushort ToShort16(this byte[] buffer, ushort precrc = 0x00)
        {
            return ToShort16(buffer, (uint)buffer.Length, precrc);
        }
        public static ushort ToShort16(this byte[] buffer, uint buffer_length, ushort precrc = 0x00)
        {

            ushort crc = precrc;
            for (uint i = 0; i < buffer_length; i++)
            {
                crc = (ushort)(crc_table[((crc >> 8) ^ buffer[i]) & 0xFF] ^ (crc << 8));
            }
            return crc;
        }


        public static byte[] StructToBytes<T>(this T obj) where T : struct
        {
            int rawsize = Marshal.SizeOf<T>();
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.StructureToPtr(obj, buffer, true);
            byte[] rawdatas = new byte[rawsize];
            Marshal.Copy(buffer, rawdatas, 0, rawsize);
            Marshal.FreeHGlobal(buffer);
            return rawdatas;
        }

        public static T BytesToStruct<T>(this byte[] bytes) where T : struct
        {
            T obj = default(T);
            Type anytype = typeof(T);
            int rawsize = Marshal.SizeOf<T>();
            if (bytes != null && bytes.Length >= rawsize)
            {
                IntPtr buffer = Marshal.AllocHGlobal(rawsize);
                Marshal.Copy(bytes, 0, buffer, rawsize);
                object retobj = Marshal.PtrToStructure(buffer, anytype);
                Marshal.FreeHGlobal(buffer);
                obj = (T)retobj;
            }
            return obj;
        }
    }
}
