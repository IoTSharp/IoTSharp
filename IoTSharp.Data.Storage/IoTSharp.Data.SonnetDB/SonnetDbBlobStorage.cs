using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SonnetDB.Data;
using SonnetDB.Data.ObjectStorage;
using SonnetDB.ObjectStorage;
using Storage.Net;
using Storage.Net.Blobs;

namespace IoTSharp.Data.SonnetDB;

public sealed class SonnetDbBlobStorage : IBlobStorage
{
    private readonly SndbObjectStorageClient _client;
    private readonly string _bucket;

    /// <summary>
    /// 创建 SonnetDB 对象存储适配器，并确保目标数据库和 bucket 已准备好。
    /// </summary>
    /// <param name="connectionString">SonnetDB.Data 连接字符串。</param>
    /// <param name="bucket">对象存储 bucket 名称。</param>
    public SonnetDbBlobStorage(string connectionString, string bucket = "iotsharp-blob-storage")
    {
        SndbResourceInitializer.EnsureDatabase(connectionString, "对象存储数据库");
        _client = new SndbObjectStorageClient(connectionString);
        _bucket = bucket;
        _client.CreateBucketAsync(bucket, SndbBucketPurpose.IoTSharpBlobStorage).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 解析 IoTSharp 使用的 <c>sonnetdb://blob</c> 对象存储连接字符串。
    /// </summary>
    /// <param name="value">包含 bucket 与 connectionString 查询参数的连接字符串。</param>
    /// <returns>SonnetDB.Data 连接字符串和 bucket 名称。</returns>
    public static (string ConnectionString, string Bucket) ParseConnectionString(string value)
    {
        var uri = new Uri(value, UriKind.Absolute);
        if (!string.Equals(uri.Scheme, "sonnetdb", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("SonnetDB BlobStorage connection string must use sonnetdb:// scheme.");
        }

        var query = ParseQuery(uri.Query);
        if (!query.TryGetValue("connectionString", out var connectionString)
            || string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("SonnetDB BlobStorage requires a connectionString query value.");
        }

        var bucket = query.TryGetValue("bucket", out var bucketValue) && !string.IsNullOrWhiteSpace(bucketValue)
            ? bucketValue
            : "iotsharp-blob-storage";

        return (connectionString, bucket);
    }

    public async Task<IReadOnlyCollection<Blob>> ListAsync(ListOptions options = null, CancellationToken cancellationToken = default)
    {
        options ??= new ListOptions { Recurse = true };
        string prefix = NormalizeListPrefix(options.FolderPath);
        var listed = await _client.ListObjectsAsync(_bucket, prefix, options.MaxResults ?? 1000, cancellationToken)
            .ConfigureAwait(false);
        var blobs = listed.Objects
            .Select(info => CreateBlob(info.Key, info))
            .Where(options.IsMatch)
            .ToList();

        return blobs;
    }

    public async Task WriteAsync(string fullPath, Stream dataStream, bool append = false, CancellationToken cancellationToken = default)
    {
        if (append)
            throw new NotSupportedException("SonnetDB BlobStorage does not support append writes.");

        await _client.PutObjectAsync(_bucket, NormalizePath(fullPath), dataStream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default)
    {
        var result = await _client.OpenReadAsync(_bucket, NormalizePath(fullPath), cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (result is null)
            throw new FileNotFoundException("Blob was not found.", fullPath);

        return result.Content;
    }

    public async Task DeleteAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default)
    {
        foreach (string fullPath in fullPaths)
        {
            await _client.DeleteObjectAsync(_bucket, NormalizePath(fullPath), cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default)
    {
        var result = new List<bool>();
        foreach (string fullPath in fullPaths)
        {
            var info = await _client.HeadObjectAsync(_bucket, NormalizePath(fullPath), cancellationToken)
                .ConfigureAwait(false);
            result.Add(info is not null);
        }

        return result;
    }

    public async Task<IReadOnlyCollection<Blob>> GetBlobsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default)
    {
        var result = new List<Blob>();
        foreach (string fullPath in fullPaths)
        {
            var info = await _client.HeadObjectAsync(_bucket, NormalizePath(fullPath), cancellationToken)
                .ConfigureAwait(false);
            if (info is not null)
                result.Add(CreateBlob(fullPath, info));
        }

        return result;
    }

    public Task SetBlobsAsync(IEnumerable<Blob> blobs, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<ITransaction> OpenTransactionAsync() =>
        throw new NotSupportedException("SonnetDB BlobStorage transactions are not supported.");

    public void Dispose() => _client.Dispose();

    private static Blob CreateBlob(string fullPath, SndbObjectInfo info)
    {
        var blob = new Blob(fullPath, BlobItemKind.File)
        {
            Size = info.SizeBytes,
            LastModificationTime = info.UpdatedUtc,
        };
        return blob;
    }

    private static string NormalizePath(string fullPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullPath);
        return fullPath.TrimStart('/').Replace('\\', '/');
    }

    private static string NormalizeListPrefix(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            return string.Empty;

        string normalized = fullPath.TrimStart('/').Replace('\\', '/');
        return normalized.Length == 0 || normalized.EndsWith("/", StringComparison.Ordinal)
            ? normalized
            : normalized + "/";
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var trimmed = query.StartsWith("?", StringComparison.Ordinal) ? query[1..] : query;
        foreach (var part in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var index = part.IndexOf('=', StringComparison.Ordinal);
            var rawKey = index >= 0 ? part[..index] : part;
            var rawValue = index >= 0 ? part[(index + 1)..] : string.Empty;
            var key = Uri.UnescapeDataString(rawKey.Replace('+', ' '));
            var value = Uri.UnescapeDataString(rawValue.Replace('+', ' '));
            if (!string.IsNullOrWhiteSpace(key))
            {
                result[key] = value;
            }
        }

        return result;
    }
}
