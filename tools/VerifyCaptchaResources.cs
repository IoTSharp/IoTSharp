#:property PublishAot=false

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: dotnet run --file tools/VerifyCaptchaResources.cs -- <IoTSharp.dll>");
    return 2;
}

const string template = "IoTSharp.Resources.buzzle-template.png";
var required = Enumerable.Range(1, 14)
    .Select(index => FormattableString.Invariant($"IoTSharp.Resources.slide{index}.jpg"))
    .Prepend(template)
    .ToHashSet(StringComparer.Ordinal);

using var assembly = File.OpenRead(args[0]);
using var pe = new PEReader(assembly);
var metadata = pe.GetMetadataReader();
var directory = pe.PEHeaders.CorHeader!.ResourcesDirectory;
if (directory.Size >= sizeof(int))
{
    var section = pe.GetSectionData(directory.RelativeVirtualAddress);
    foreach (var handle in metadata.ManifestResources)
    {
        var resource = metadata.GetManifestResource(handle);
        var name = metadata.GetString(resource.Name);
        if (!required.Contains(name) || !resource.Implementation.IsNil ||
            resource.Offset < 0 || resource.Offset > directory.Size - sizeof(int))
        {
            continue;
        }

        // CLR 资源条目以 4 字节长度开头，后接嵌入的文件内容。
        var offset = (int)resource.Offset;
        var content = section.GetReader(offset, directory.Size - offset);
        var length = content.ReadInt32();
        ReadOnlySpan<byte> signature = name == template
            ? [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]
            : [0xFF, 0xD8, 0xFF];
        if (length > signature.Length && length <= content.RemainingBytes &&
            content.ReadBytes(signature.Length).AsSpan().SequenceEqual(signature))
        {
            required.Remove(name);
        }
    }
}

if (required.Count > 0)
{
    Console.Error.WriteLine("Missing or invalid embedded CAPTCHA resources: " +
        string.Join(", ", required.Order(StringComparer.Ordinal)));
    return 1;
}

Console.WriteLine("Verified all 15 embedded CAPTCHA image resources.");
return 0;
