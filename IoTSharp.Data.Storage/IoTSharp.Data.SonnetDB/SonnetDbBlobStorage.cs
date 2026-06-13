using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SonnetDB.Data.ObjectStorage;
using SonnetDB.ObjectStorage;
using Storage.Net;
using Storage.Net.Blobs;

namespace IoTSharp.Data.SonnetDB;

public sealed class SonnetDbBlobStorage : IBlobStorage
{
    private readonly SndbObjectStorageClient _client;
    private readonly string _bucket;

    public SonnetDbBlobStorage(string connectionString, string bucket = "iotsharp-blob-storage")
    {
        _client = new SndbObjectStorageClient(connectionString);
        _bucket = bucket;
        _client.CreateBucketAsync(bucket, SndbBucketPurpose.IoTSharpBlobStorage).GetAwaiter().GetResult();
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
}
