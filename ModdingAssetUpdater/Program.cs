global using System.Collections.Frozen;
global using System.Diagnostics;
global using System.IO.Compression;

const string OutputFolder = @"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources";

const string InputFolder = @"D:\Software\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\StreamingAssets\Modding";
const string AssetRipperFolder = @"D:\Personal\Mods\Timberborn\V1DataRipping\ExportedProject\Assets\Resources";
const string GameAssembliesPath = @"D:\Software\SteamLibrary\steamapps\common\Timberborn\Timberborn_Data\Managed";
const string DecompileOutputFolder = "out";
string[] GameAssemblyPrefixes = ["Bindito.", "Timberborn."];
var maxParallelDecompilers = Math.Max(1, Environment.ProcessorCount);

// 1. Delete the existing output, but not the folder itself
if (Directory.Exists(OutputFolder))
{
    foreach (var file in Directory.EnumerateFiles(OutputFolder))
    {
        File.Delete(file);
    }

    foreach (var folder in Directory.EnumerateDirectories(OutputFolder))
    {
        Directory.Delete(folder, true);
    }
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

// 5. Decompile game DLLs
var decompileOutputFolder = Path.Combine(FindCsProjFolder(Environment.CurrentDirectory), DecompileOutputFolder);

if (Directory.Exists(decompileOutputFolder))
{
    Directory.Delete(decompileOutputFolder, true);
    await Task.Delay(500);
}

Directory.CreateDirectory(decompileOutputFolder);

ParallelOptions decompileOptions = new()
{
    MaxDegreeOfParallelism = maxParallelDecompilers,
};

await Parallel.ForEachAsync(GetGameAssemblyPaths(), decompileOptions, async (dll, _) =>
{
    var assemblyName = Path.GetFileNameWithoutExtension(dll);
    var assemblyOutputFolder = Path.Combine(decompileOutputFolder, assemblyName);

    Console.WriteLine($"Decompiling {assemblyName} to {assemblyOutputFolder}");
    await RunIlSpyAsync(dll, assemblyOutputFolder);
});

Console.WriteLine("Done");

IEnumerable<string> GetGameAssemblyPaths()
{
    foreach (var prefix in GameAssemblyPrefixes)
    {
        foreach (var dll in Directory.EnumerateFiles(GameAssembliesPath, $"{prefix}*.dll", SearchOption.AllDirectories))
        {
            yield return dll;
        }
    }
}

static async Task RunIlSpyAsync(string dll, string outputFolder)
{
    Directory.CreateDirectory(outputFolder);

    ProcessStartInfo startInfo = new()
    {
        FileName = "ilspycmd",
        UseShellExecute = false,
    };

    startInfo.ArgumentList.Add("-p");
    startInfo.ArgumentList.Add("-o");
    startInfo.ArgumentList.Add(outputFolder);
    startInfo.ArgumentList.Add(dll);

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start ilspycmd.");

    await process.WaitForExitAsync();

    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException($"ilspycmd failed for '{dll}' with exit code {process.ExitCode}.");
    }
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
