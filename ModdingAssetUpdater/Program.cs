global using System.Collections.Frozen;
global using System.IO.Compression;

const string OutputFolder = @"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources";

const string InputFolder = @"D:\Software\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\StreamingAssets\Modding";
const string AssetRipperFolder = @"D:\Personal\Mods\Timberborn\V1DataRipping\ExportedProject\Assets\Resources";

// 1. Delete the folder
if (Directory.Exists(OutputFolder))
{
    Directory.Delete(OutputFolder, true);
}

// 2. Extract the zip files
Dictionary<string, string> ZipFiles = new(){
    { "Blueprints.zip", "" },
    { "Localizations.zip", "Localizations" },
    { "Shaders.zip", "Shaders" },
    { "UI.zip", "UI" },
};

foreach (var (name, path) in ZipFiles)
{
    var outputPath = Path.Combine(OutputFolder, path);
    Console.WriteLine($"Extracting {name} to {outputPath}");
    Directory.CreateDirectory(outputPath);

    await using var zipStream = File.OpenRead(Path.Combine(InputFolder, name));
    await using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);
    await zipArchive.ExtractToDirectoryAsync(outputPath);
}

// 3. Copy the ripped assets
FrozenSet<string> CopyingExtensions = ((string[])[".prefab", ".mat", ".png"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
Stack<string> rippedPaths = new([""]);

Console.WriteLine("Copying ripped assets...");
while (rippedPaths.Count > 0)
{
    var subPath = rippedPaths.Pop();
    var fullPath = Path.Combine(AssetRipperFolder, subPath);

    foreach (var subFolder in Directory.EnumerateDirectories(fullPath))
    {
        var name = Path.GetFileName(subFolder);
        rippedPaths.Push(Path.Combine(subPath, name));
    }

    var outputPath = Path.Combine(OutputFolder, subPath);
    Directory.CreateDirectory(outputPath);
    foreach (var file in Directory.EnumerateFiles(fullPath))
    {
        var ext = Path.GetExtension(file);
        if (!CopyingExtensions.Contains(ext)) { continue; }

        var name = Path.GetFileName(file);
        Console.WriteLine($"Copying {name} to {outputPath}");
        File.Copy(file, Path.Combine(outputPath, name));
    }
}

// 4. Copy Placeholder Shaders to map later
Console.WriteLine("Copying placeholder shaders...");

var rippedShaderFolder = Path.Combine(AssetRipperFolder, "../Shader");
var dstPlaceholderShader = Path.Combine(OutputFolder, "ShaderPlaceholders");
Directory.CreateDirectory(dstPlaceholderShader);

foreach (var path in Directory.EnumerateFiles(rippedShaderFolder))
{
    var fileName = Path.GetFileName(path);
    var dst = Path.Combine(dstPlaceholderShader, fileName);
    File.Copy(path, dst);

    if (!fileName.EndsWith(".shader")) { continue; }

    var lines = await File.ReadAllLinesAsync(dst);
    lines[0] = lines[0].Replace("\" {", "2\" {");
    await File.WriteAllLinesAsync(dst, lines);
}

Console.WriteLine("Done");