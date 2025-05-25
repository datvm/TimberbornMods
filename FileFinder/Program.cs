using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Text.Json;

const string FileInput = @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\wellbeing\decontaminationpod\DecontaminationPod.IronTeeth.prefab";
string input = "";

if (!string.IsNullOrEmpty(FileInput))
{
    input = await File.ReadAllTextAsync(FileInput);
}

FileMapper mapper = new();
await mapper.LoadAsync();

List<(string, GameFile)?> output = [];
HashSet<string> parsedGuid = [];
foreach (var line in input.Split('\n'))
{
    var guid = ParseForGuid(line);
    if (guid is null || parsedGuid.Contains(guid)) { continue; }

    parsedGuid.Add(guid);
    if (mapper.FilesByGuid.TryGetValue(guid, out var file))
    {
        output.Add((guid, file));
    }
    else
    {
        output.Add(null);
    }
}

foreach (var o in output)
{
    if (o is null) { continue; }
    var (guid, f) = o.Value;

    var print = f.Path[FileMapper.AssetFolder.Length..];
    if (f.Path.EndsWith(".mat"))
    {
        print = print.Replace("Resources/", "").Replace(".mat", "");
        print = $"\"{print}\"";
    }
    else
    {
        print = $"{guid}: {print}";
    }

    Console.WriteLine(print);
}


string? ParseForGuid(string line)
{
    if (!line.Contains(' ')) { return line; }

    var start = line.IndexOf("guid: ");
    if (start > -1)
    {
        var actualStart = start + "guid: ".Length;
        var end = line.IndexOf(',', actualStart + 1);

        var guid = line[actualStart..end];
        Console.WriteLine($"Found {guid} from {line}");
        return guid;
    }

    return null;
}

public class FileMapper
{
    public const string AssetFolder = @"D:/Personal/Mods/Timberborn/U7Data/ExportedProject/Assets/";
    const string FileName = "map.json";

    public FrozenDictionary<string, GameFile> FilesByGuid { get; set; } = null!;
    public FrozenDictionary<string, GameFile> FilesByPath { get; set; } = null!;

    public async Task LoadAsync()
    {
        var files = await GetGameFileAsync();

        FilesByGuid = files.ToFrozenDictionary(q => q.Guid);
        FilesByPath = files.ToFrozenDictionary(q => q.Path);
    }

    static async Task<IEnumerable<GameFile>> GetGameFileAsync()
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