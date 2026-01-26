using System.IO.Compression;

const string OutputFolder = @"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources";
const string InputFolder = @"D:\Software\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\StreamingAssets\Modding";

Console.WriteLine("Performing dry run...");

await RunAsync(true);

Console.Write("Press ENTER to actually perform the updates...");
Console.ReadLine();

await RunAsync(false);

static async Task RunAsync(bool dryRun)
{
    await UpdateFileAsync("Blueprints.zip", "", dryRun);
    await UpdateFileAsync("UI.zip", "UI", dryRun);
}

static async Task UpdateFileAsync(string inputFile, string outputFolder, bool dryRun)
{
    await using var stream = File.OpenRead(Path.Combine(InputFolder, inputFile));
    await using var zip = new ZipArchive(stream, ZipArchiveMode.Read);

    outputFolder = Path.Combine(OutputFolder, outputFolder);

    var rootFolders = GetRootFolders(zip);
    foreach (var f in rootFolders)
    {
        var fPath = Path.Combine(outputFolder, f);
        if (!Directory.Exists(fPath))
        {
            Console.WriteLine($"- {fPath} does not exist yet.");
            continue;
        }

        Console.WriteLine($"- Deleting {fPath}");
        if (!dryRun)
        {
            Directory.Delete(fPath, true);
        }
    }

    Console.WriteLine($"- Extracting {inputFile}...");
    if (!dryRun)
    {
        await zip.ExtractToDirectoryAsync(outputFolder);
    }
}

static HashSet<string> GetRootFolders(ZipArchive zip)
{
    var rootFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    foreach (var entry in zip.Entries)
    {
        var root = entry.FullName.Split(['\\', '/'])[0];

        if (!string.IsNullOrEmpty(root))
        {
            rootFolders.Add(root);
        }
    }

    return rootFolders;
}
