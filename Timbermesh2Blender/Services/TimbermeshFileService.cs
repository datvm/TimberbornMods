namespace Timbermesh2Blender.Services;

using System.IO.Compression;
using ProtoBuf;

public static class TimbermeshFileService
{
    const byte FirstZLibHeaderByte = 120;
    const byte SecondZLibHeaderByte = 156;

    public static async Task<TimbermeshFile?> TryParseAsync(string filePath)
    {
        if (filePath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
        {
            return await TryParsePrefabFileAsync(filePath);
        }

        if (filePath.EndsWith(".timbermesh", StringComparison.OrdinalIgnoreCase))
        {
            await using var stream = File.OpenRead(filePath);
            return await TryParseTimbermeshBinaryAsync(filePath, stream);
        }

        return null;
    }

    public static async Task<TimbermeshFile?> TryParsePrefabFileAsync(string filePath)
    {
        await foreach (var line in ReadLinesAsync(filePath))
        {
            var i = line.IndexOf("_bytes: ", StringComparison.Ordinal);
            if (i < 0)
            {
                continue;
            }

            var content = line[(i + "_bytes: ".Length)..].Trim();
            if (content.Length == 0)
            {
                return null;
            }

            try
            {
                var bytes = Convert.FromHexString(content);
                await using MemoryStream stream = new(bytes);
                return await TryParseTimbermeshBinaryAsync(filePath, stream);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    public static async Task<TimbermeshFile?> TryParseTimbermeshBinaryAsync(string filePath, Stream stream)
    {
        try
        {
            await using var source = await GetDecompressedStreamAsync(stream);
            var model = Serializer.Deserialize<Model>(source);
            return new TimbermeshFile(filePath, model);
        }
        catch
        {
            return null;
        }
    }

    static async Task<MemoryStream> GetDecompressedStreamAsync(Stream stream)
    {
        ValidateFileHeader(stream);

        var memoryStream = new MemoryStream();
        await using var deflateStream = new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: true);
        await deflateStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0L;
        return memoryStream;
    }

    static void ValidateFileHeader(Stream stream)
    {
        if (stream.ReadByte() != FirstZLibHeaderByte || stream.ReadByte() != SecondZLibHeaderByte)
        {
            throw new InvalidDataException("Incorrect Zlib compression file header");
        }
    }

    static async IAsyncEnumerable<string> ReadLinesAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync() is { } line)
        {
            yield return line;
        }
    }
}
