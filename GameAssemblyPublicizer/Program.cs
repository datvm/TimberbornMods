using BepInEx.AssemblyPublicizer;
using System.Collections.Immutable;

ImmutableArray<string> Prefixes = ["Bindito.", "Timberborn.", "Unity"];
const string GameAssembliesPath = @"E:\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\Managed";
ImmutableArray<string> SpecialFolders = [
    @"E:\SteamLibrary\steamapps\workshop\content\1062090\3283831040\version-0.7\Scripts",
];

var outputFolder = Path.Combine(FindCsProjFolder(Environment.CurrentDirectory), "out");
Directory.CreateDirectory(outputFolder);

foreach (var prefix in Prefixes)
{
    foreach (var dll in Directory.EnumerateFiles(GameAssembliesPath, $"{prefix}*.dll"))
    {
        PublicizeFile(dll);
    }
}

foreach (var folder in SpecialFolders)
{
    foreach (var file in Directory.EnumerateFiles(folder, "*.dll"))
    {
        PublicizeFile(file);
    }
}

void PublicizeFile(string dll)
{
    var name = Path.GetFileName(dll);
    var output = Path.Combine(outputFolder, name);

    Console.WriteLine($"Publicizing {name} to {output}");
    AssemblyPublicizer.Publicize(dll, output, new()
    {
        IncludeOriginalAttributesAttribute = true,
        Strip = true,
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
