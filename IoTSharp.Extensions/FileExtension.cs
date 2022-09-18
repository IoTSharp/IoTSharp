using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace IoTSharp.Extensions
{
    public static class FileExtension
    {
        /// <summary>
        /// 如果文件存在， 则返回长度， 不存在， 则返回 0 。 
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public static long GetLength(this FileInfo fi) => fi.Exists ? fi.Length : 0;

        /// <summary>
        /// 使用 Filter列举文件
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="filesFilter"></param>
        /// <param name="searchOption"></param>
        /// <param name="limit">数量上限， 如果大于0 ， 则表示只取列出的前<para>limit </para></param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateFilesFilter(this DirectoryInfo directory,  string  filesFilter, SearchOption searchOption = SearchOption.TopDirectoryOnly, int limit = -1)
        {
            if (limit == -1)
            {
                return filesFilter.Split(',', ';', '|').SelectMany(_ => Directory.EnumerateFiles(directory.FullName, "*" + _, searchOption));
            }
            else
            {
                List<string> lst = new List<string>();
                foreach (var item in filesFilter.Split(',', ';', '|'))
                {
                    lst.AddRange(Directory.EnumerateFiles(directory.FullName, item, searchOption).Take(limit));
                    if (lst.Count >= limit)
                    {
                        break;
                    }
                }
                return lst;
            }
        }
        public static DriveInfo GetDriveInfo(this FileInfo file)
        {
            return new DriveInfo(file.Directory.Root.FullName);
        }
   
       

        public static byte[] ReadAllBytes(this FileInfo fi) => File.ReadAllBytes(fi.FullName);
        public static byte[] ReadBytes(this FileInfo fi, int count) => ReadBytes(fi, 0, count);


        public static byte[] ReadBytes(this FileInfo fi, int offset, int count)
        {
            byte[] buffer = new byte[count];
            using (var fs = fi.OpenRead())
            {
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Read(buffer, 0, count);
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
