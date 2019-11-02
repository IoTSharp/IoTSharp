using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IoTSharp.Extensions
{
    public static class FileExtension
    {

        public static string GetSHA1(this FileInfo s)
        {
            string result = string.Empty;
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] retval = sha1.ComputeHash(s.ReadAllBytes());
            result = BitConverter.ToString(retval).Replace("-", "");
            return result;
        }
        public static string GetMd5Sum(this FileInfo s)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string t2 = BitConverter.ToString(md5.ComputeHash(s.ReadAllBytes()));
            t2 = t2.Replace("-", "");
            return t2;
        }

        public static byte[] ReadAllBytes(this FileInfo fi) => File.ReadAllBytes(fi.FullName);
        public static byte[] ReadBytes(this FileInfo fi, int count) => ReadBytes(fi, 0, count);


        public static byte[] ReadBytes(this FileInfo fi, int offset, int count)
        {
            byte[] buffer = new byte[count];
            using (var fs = fi.OpenRead())
            {
                fs.Read(buffer, offset, count);
            }
            return buffer;
        }

        public static void WriteAllText(this FileInfo fi, string contents) => File.WriteAllText(fi.FullName, contents);
        public static bool Exists(this FileInfo fi) => File.Exists(fi.FullName);
        public static void Delete(this FileInfo fi) => File.Delete(fi.FullName);
        public static string ReadAllText(this FileInfo fi) => File.ReadAllText(fi.FullName);
        public static void WriteAllBytes(this FileInfo fi, byte[] bytes) => File.WriteAllBytes(fi.FullName, bytes);
    }
}
