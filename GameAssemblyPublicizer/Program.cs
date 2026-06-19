using BepInEx.AssemblyPublicizer;
using System.Collections.Immutable;

ImmutableArray<string> Prefixes = ["Bindito.", "Timberborn.", "Unity"];
// Container/CI-friendly: override via GAME_MANAGED_PATH; falls back to the original local-dev Windows path.
string GameAssembliesPath = Environment.GetEnvironmentVariable("GAME_MANAGED_PATH")
    ?? @"D:\Software\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\Managed";
ImmutableArray<string> SpecialFolders = [
    @"D:\Software\SteamLibrary\steamapps\workshop\content\1062090\3283831040\version-1.0\Scripts", // Mod Settings
];
ImmutableArray<KeyValuePair<string, string>> OtherMods = [
    //new(@"D:\Software\SteamLibrary\steamapps\workshop\content\1062090\3275060459\version-0.7\Scripts", "ShantySpeaker"),
    //new(@"C:\Users\lukev\OneDrive\Documents\Timberborn\Mods\ModdableWeather\version-0.7", "ModdableWeather"),
];

var outputFolder = Path.Combine(FindCsProjFolder(Environment.CurrentDirectory), "out");
var commonOutput = Path.Combine(outputFolder, "common");

if (Directory.Exists(outputFolder))
{
    Directory.Delete(outputFolder, true);
    await Task.Delay(500);
}

Directory.CreateDirectory(commonOutput);

// Common
foreach (var prefix in Prefixes)
{
    foreach (var dll in Directory.EnumerateFiles(GameAssembliesPath, $"{prefix}*.dll", SearchOption.AllDirectories))
    {
        PublicizeFile(dll, commonOutput);
    }
}

// Extra special folders from env (container/CI), PathSeparator-delimited — e.g. the Mod Settings 1.1 Scripts dir.
var extraDirs = (Environment.GetEnvironmentVariable("EXTRA_PUBLICIZE_DIRS") ?? "")
    .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

foreach (var folder in SpecialFolders.Concat(extraDirs))
{
    // Skip missing folders so cross-platform / container runs don't crash on local-dev-only paths.
    if (!Directory.Exists(folder))
    {
        Console.WriteLine($"Skipping missing special folder: {folder}");
        continue;
    }
    foreach (var file in Directory.EnumerateFiles(folder, "*.dll", SearchOption.AllDirectories))
    {
        PublicizeFile(file, commonOutput);
    }
}

// Other mods
foreach (var (folder, name) in OtherMods)
{
    var specialOutput = Path.Combine(outputFolder, name);
    Directory.CreateDirectory(specialOutput);

    foreach (var dll in Directory.EnumerateFiles(folder, "*.dll", SearchOption.AllDirectories))
    {
        PublicizeFile(dll, specialOutput);
    }
}

void PublicizeFile(string dll, string outputFolder)
{
    var name = Path.GetFileName(dll);
    var output = Path.Combine(outputFolder, name);

    Console.WriteLine($"Publicizing {name} to {outputFolder}");
    AssemblyPublicizer.Publicize(dll, output, new()
    {
        IncludeOriginalAttributesAttribute = true,
        Strip = false,
    });
}

// Look for csproj file here or in parent directories
static string FindCsProjFolder(string folder)
{
    while (true)
    {
        var csproj = Directory.EnumerateFiles(folder, "*.csproj").FirstOrDefault();
        if (csproj != null)
        {
            return Path.GetDirectoryName(csproj)!;
        }
        var parent = Path.GetDirectoryName(folder) ?? throw new InvalidOperationException("Could not find csproj file");
        folder = parent;
    }
}
