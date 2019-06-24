using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace IoTSharp.Storage
{
    public class StorageService 
    {
        private readonly JsonSerializerService _jsonSerializerService;
        
        public StorageService(JsonSerializerService jsonSerializerService, ILogger<StorageService> logger)
        {
            _jsonSerializerService = jsonSerializerService ?? throw new ArgumentNullException(nameof(jsonSerializerService));
            
            var paths = new StoragePaths();
            BinPath = paths.BinPath;
            DataPath = paths.DataPath;

            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.Log(LogLevel.Information, $"Bin path  = {BinPath}");
            logger.Log(LogLevel.Information, $"Data path = {DataPath}");
        }

        public string BinPath { get; }

        public string DataPath { get; }

        public void Start()
        {
        }

        public List<string> EnumerateDirectories(string pattern, params string[] path)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            if (path == null) throw new ArgumentNullException(nameof(path));

            var directory = Path.Combine(DataPath, Path.Combine(path));
            if (!Directory.Exists(directory))
            {
                return new List<string>();
            }

            var directories = Directory.EnumerateDirectories(directory, pattern, SearchOption.TopDirectoryOnly).ToList();
            for (var i = 0; i < directories.Count; i++)
            {
                directories[i] = directories[i].Replace(directory, string.Empty).TrimStart(Path.DirectorySeparatorChar);
            }

            return directories;
        }

        public List<string> EnumerateFiles(string pattern, params string[] path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var relativePath = Path.Combine(path);
            var directory = Path.Combine(DataPath, relativePath);

            if (!Directory.Exists(directory))
            {
                return new List<string>();
            }

            var files = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories).ToList();
            for (var i = 0; i < files.Count; i++)
            {
                files[i] = files[i].Replace(directory, string.Empty).TrimStart(Path.DirectorySeparatorChar);
            }

            return files;
        }

        public bool TryRead<TValue>(out TValue value, params string[] path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var filename = Path.Combine(DataPath, Path.Combine(path));
            if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".json";
            }

            if (!File.Exists(filename))
            {
                value = default(TValue);
                return false;
            }

            var json = File.ReadAllText(filename, Encoding.UTF8);
            if (string.IsNullOrEmpty(json))
            {
                value = default(TValue);
                return true;
            }

            value = _jsonSerializerService.Deserialize<TValue>(json);
            return true;
        }

        public bool TryReadText(out string value, params string[] path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var filename = Path.Combine(DataPath, Path.Combine(path));
            if (!File.Exists(filename))
            {
                value = null;
                return false;
            }

            value = File.ReadAllText(filename, Encoding.UTF8);
            return true;
        }

        public bool TryReadBinText(out string value, params string[] path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var filename = Path.Combine(BinPath, Path.Combine(path));
            if (!File.Exists(filename))
            {
                value = null;
                return false;
            }

            value = File.ReadAllText(filename, Encoding.UTF8);
            return true;
        }

        public bool TryReadRaw(out byte[] content, params string[] path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var filename = Path.Combine(DataPath, Path.Combine(path));
            if (!File.Exists(filename))
            {
                content = null;
                return false;
            }

            content = File.ReadAllBytes(filename);
            return true;
        }

        public bool TryReadOrCreate<TValue>(out TValue value, params string[] path) where TValue : class, new()
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (!TryRead(out value, path))
            {
                value = new TValue();
                Write(value, path);
                return false;
            }

            return true;
        }

        public void Write(object value, params string[] path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var filename = Path.Combine(DataPath, Path.Combine(path));
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

            var json = _jsonSerializerService.Serialize(value);
            File.WriteAllText(filename, json, Encoding.UTF8);
        }

        public void WriteRaw(byte[] content, params string[] path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var filename = Path.Combine(DataPath, Path.Combine(path));
            var directory = Path.GetDirectoryName(filename);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filename, content ?? new byte[0]);
        }

        public void WriteText(string value, params string[] path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var filename = Path.Combine(DataPath, Path.Combine(path));
            var directory = Path.GetDirectoryName(filename);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filename, value ?? string.Empty, Encoding.UTF8);
        }

        public void DeleteFile(params string[] path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var fullPath = Path.Combine(DataPath, Path.Combine(path));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public void DeleteDirectory(params string[] path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var fullPath = Path.Combine(DataPath, Path.Combine(path));
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
        }
    }
}
