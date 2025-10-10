using BepInEx.AssemblyPublicizer;
using System.Collections.Immutable;

ImmutableArray<string> Prefixes = ["Bindito.", "Timberborn.", "Unity"];
const string GameAssembliesPath = @"D:\Software\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\Managed";
ImmutableArray<string> SpecialFolders = [
    @"D:\Software\SteamLibrary\steamapps\workshop\content\1062090\3283831040\version-0.7\Scripts", // Mod Settings
];
ImmutableArray<KeyValuePair<string, string>> OtherMods = [
    new(@"D:\Software\SteamLibrary\steamapps\workshop\content\1062090\3275060459\version-0.7\Scripts", "ShantySpeaker"),
    new(@"C:\Users\lukev\OneDrive\Documents\Timberborn\Mods\ModdableWeather\version-0.7", "ModdableWeather"),
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

foreach (var folder in SpecialFolders)
{
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
