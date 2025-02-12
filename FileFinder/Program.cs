using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Text.Json;

const string Input = """
- {fileID: 2100000, guid: 69d2a00d4a80477439646fe15a973c1e, type: 2}
- {fileID: 2100000, guid: d2ca7622ecf71a8408ba137aea34dbbe, type: 2}
- {fileID: 2100000, guid: 285699b655cfeee4fad87b1451821e46, type: 2}
- {fileID: 2100000, guid: 3266c26cda0db044da2e80ce40729b61, type: 2}
- {fileID: 2100000, guid: 285699b655cfeee4fad87b1451821e46, type: 2}
- {fileID: 2100000, guid: 69d2a00d4a80477439646fe15a973c1e, type: 2}
- {fileID: 2100000, guid: 051ad856be43a8547844c211ff5385d8, type: 2}
- {fileID: 2100000, guid: fdda7216865ae23408680bc16c128d62, type: 2}
- {fileID: 2100000, guid: d2ca7622ecf71a8408ba137aea34dbbe, type: 2}
- {fileID: 2100000, guid: d2ca7622ecf71a8408ba137aea34dbbe, type: 2}
""";

FileMapper mapper = new();
await mapper.LoadAsync();

List<GameFile?> output = [];
foreach (var line in Input.Split('\n'))
{
    var guid = ParseForGuid(line);
    if (guid is null) { continue; }

    if (mapper.FilesByGuid.TryGetValue(guid, out var file))
    {
        output.Add(file);
    }
    else
    {
        output.Add(null);
    }
}

Console.WriteLine(string.Join(Environment.NewLine,
    output
        .Select(q => $"\"{q?.Path[FileMapper.AssetFolder.Length..] ?? "N/A"}\",")
        .Distinct()
));

string? ParseForGuid(string line)
{
    if (line.IndexOf(' ') == -1) { return line; }

    var start = line.IndexOf("guid: ");
    if (start > -1)
    {
        var actualStart = start + "guid: ".Length;
        var end = line.IndexOf(',', actualStart + 1);

        return line[actualStart..end];
    }

    return null;
}

public class FileMapper
{
    public const string AssetFolder = @"D:/Personal/Mods/Timberborn/U7Data/ExportedProject/Assets/Resources/";
    const string FileName = "map.json";

    public FrozenDictionary<string, GameFile> FilesByGuid { get; set; } = null!;
    public FrozenDictionary<string, GameFile> FilesByPath { get; set; } = null!;

    public async Task LoadAsync()
    {
        var files = await GetGameFileAsync();

        FilesByGuid = files.ToFrozenDictionary(q => q.Guid);
        FilesByPath = files.ToFrozenDictionary(q => q.Path);
    }

    async Task<IEnumerable<GameFile>> GetGameFileAsync()
    {
        if (File.Exists(FileName))
        {
            await using var f = File.OpenRead(FileName);
            return await JsonSerializer.DeserializeAsync<IEnumerable<GameFile>>(f)
                ?? throw new InvalidOperationException("Failed to deserialize file.");
        }

        Console.WriteLine("Mapping files...");
        ConcurrentBag<GameFile> files = [];

        await Parallel.ForEachAsync(
            Directory.EnumerateFiles(AssetFolder, "*.meta", SearchOption.AllDirectories), 
            new ParallelOptions()
            {
                MaxDegreeOfParallelism = 1,
            },
            async (path, ct) =>
            {
                await using var f = File.OpenRead(path);
                using var reader = new StreamReader(f);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync(ct);
                    if (line is null) { return; }

                    if (line.StartsWith("guid"))
                    {
                        var guid = line.Split(' ')[1];

                        path = path[..^".meta".Length].Replace('\\', '/');
                        files.Add(new(guid,
                            path, 
                            Path.GetFileName(path)));
                        return;
                    }
                }                
            });

        await using var fs = File.Create(FileName);
        await JsonSerializer.SerializeAsync(fs, files);

        return files;
    }

}

public readonly record struct GameFile(string Guid, string Path, string Name);