using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IoTSharp.Extensions
{
    public static class StorageExtensions
    {

        public static bool TryRead<TValue>(this FileInfo file , out TValue value)
        {
            if (!file.Exists)
            {
                value = default(TValue);
                return false;
            }

            var json = System.IO.File.ReadAllText(file.FullName);
            if (string.IsNullOrEmpty(json))
            {
                value = default(TValue);
                return true;
            }
            value =Newtonsoft.Json.JsonConvert.DeserializeObject<TValue>(json);
            return true;
        }

        public static bool TryReadText(this FileInfo file,  out string value )
        {
              value = File.ReadAllText(file.FullName);
            return true;
        }

        public static bool TryReadBinText(this FileInfo file, out string value)
        {

            var filename = file.FullName;
            if (!File.Exists(filename))
            {
                value = null;
                return false;
            }

            value = File.ReadAllText(filename, Encoding.UTF8);
            return true;
        }

        public static bool TryReadRaw(this FileInfo file,  out byte[] content)
        {
            var filename = file.FullName;
            if (!File.Exists(filename))
            {
                content = null;
                return false;
            }

            content = File.ReadAllBytes(filename);
            return true;
        }

        public static bool TryReadOrCreate<TValue>(this FileInfo file, out TValue value) where TValue : class, new()
        {

            if (!file.TryRead(out value))
            {
                value = new TValue();
                file.Write(value);
                return false;
            }
            return true;
        }

        public static void Write(this FileInfo file,  object value)
        {

            var filename = file.FullName;
            if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".json";
            }

            var directory = Path.GetDirectoryName(filename);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (value == null)
            {
                File.WriteAllBytes(filename, new byte[0]);
                return;
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            File.WriteAllText(filename, json);
        }

        public static void WriteRaw(this FileInfo file,  byte[] content)
        {

            var filename = file.FullName;
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            File.WriteAllBytes(filename, content ?? new byte[0]);
        }
    }
}
