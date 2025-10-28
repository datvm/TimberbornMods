namespace TimbermeshMaterialPatcher;

public static class TimbermeshReader
{
    static readonly byte FirstZLibHeaderByte = 120;

    static readonly byte SecondZLibHeaderByte = 156;

    /// <summary>
    /// Parses a Timbermesh model from a file. Supports both .prefab files and binary (.timbermesh) files.
    /// </summary>
    public static Model ParseFile(string filePath)
    {
        if (filePath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
        {
            return ReadFromPrefab(filePath);
        }
        else
        {
            using var stream = File.OpenRead(filePath);
            return ReadFromStream(stream);
        }
    }

    public static Model ReadFromStream(Stream stream)
    {
        using MemoryStream source = GetDecompressedStream(stream);
        return Serializer.Deserialize<Model>(source);
    }

    public static Model ReadFromPrefab(string prefabFilePath)
    {
        return ReadFromPrefab(ReadLines(prefabFilePath));

        static IEnumerable<string> ReadLines(string path)
        {
            using var stream = File.OpenRead(path);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine() ?? string.Empty;
            }
        }
    }

    public static Model ReadFromPrefab(IEnumerable<string> lines)
    {
        Stream? stream = null;
        foreach (var l in lines)
        {
            var i = l.IndexOf("_bytes: ");
            if (i > -1)
            {
                var content = l[(i + "_bytes: ".Length)..].Trim();
                var bytes = Convert.FromHexString(content);
                stream = new MemoryStream(bytes);
                break;
            }
        }

        if (stream is null)
        {
            throw new Exception("Could not find _bytes field in prefab");
        }

        return ReadFromStream(stream);
    }

    static MemoryStream GetDecompressedStream(Stream stream)
    {
        ValidateFileHeader(stream);
        var memoryStream = new MemoryStream();
        using var deflateStream = new DeflateStream(stream, CompressionMode.Decompress, leaveOpen: true);
        deflateStream.CopyTo(memoryStream);
        memoryStream.Position = 0L;
        return memoryStream;
    }

    static void ValidateFileHeader(Stream stream)
    {
        if (stream.ReadByte() != FirstZLibHeaderByte || stream.ReadByte() != SecondZLibHeaderByte)
        {
            throw new Exception("Incorrect Zlib compression file header");
        }
    }

    public static async Task WriteToStreamAsync(Model model, Stream stream)
    {
        await using MemoryStream memoryStream = new();
        Serializer.Serialize(memoryStream, model);
        memoryStream.Position = 0L;

        await using DeflateStream deflateStream = new(stream, CompressionMode.Compress, leaveOpen: true);
        stream.WriteByte(FirstZLibHeaderByte);
        stream.WriteByte(SecondZLibHeaderByte);
        memoryStream.CopyTo(deflateStream);
    }
}