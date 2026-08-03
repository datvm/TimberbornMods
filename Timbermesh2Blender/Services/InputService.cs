namespace Timbermesh2Blender.Services;

public static class InputService
{
    public static ScriptInput GetInput(string[] args)
    {
        var inputOption = new Option<FileSystemInfo>("-i", "--input")
        {
            Description = "Timbermesh file or folder containing timbermesh models",
        };
        inputOption.AcceptExistingOnly();

        var textureOption = new Option<DirectoryInfo>("-r", "--resources")
        {
            Description = "Resources folder containing textures / game assets",
        };
        textureOption.AcceptExistingOnly();

        var outputOption = new Option<string>("-o", "--output")
        {
            Description = "Folder to write Blender-friendly output",
        };

        var flattenOption = new Option<bool>("-f", "--flatten")
        {
            Description = "Write all .glb files directly into the output folder (do not keep input folder structure)",
        };

        var root = new RootCommand("Convert Timbermesh models to a Blender-friendly format")
        {
            inputOption,
            textureOption,
            outputOption,
            flattenOption,
        };

        var parseResult = root.Parse(args);
        if (parseResult.Action is not null)
        {
            var exitCode = parseResult.Invoke();
            Environment.Exit(exitCode);
        }

        if (parseResult.Errors.Count > 0)
        {
            foreach (var error in parseResult.Errors)
            {
                Console.Error.WriteLine(error.Message);
            }

            throw new ArgumentException("Invalid command line arguments.");
        }

        var inputPath = RequireExistingPath(parseResult.GetValue(inputOption)?.FullName, "Input path (file or folder)");
        var textureFolder = RequireExistingDirectory(parseResult.GetValue(textureOption)?.FullName, "Texture folder");
        var outputFolder = Require(parseResult.GetValue(outputOption), "Output folder");
        var flatten = parseResult.GetValue(flattenOption);

        return new(inputPath, textureFolder, outputFolder, flatten);
    }

    public static async IAsyncEnumerable<TimbermeshFile> GetTimbermeshFilesAsync(string inputPath)
    {
        if (File.Exists(inputPath))
        {
            var file = await TimbermeshFileService.TryParseAsync(inputPath);
            if (file is not null)
            {
                yield return file;
            }
            else
            {
                Console.Error.WriteLine($"Not a timbermesh file: {inputPath}");
            }

            yield break;
        }

        if (!Directory.Exists(inputPath))
        {
            throw new DirectoryNotFoundException($"Input path does not exist: {inputPath}");
        }

        foreach (var path in EnumerateTimbermeshCandidateFiles(inputPath))
        {
            var file = await TimbermeshFileService.TryParseAsync(path);
            if (file is not null)
            {
                yield return file;
            }
        }
    }

    static IEnumerable<string> EnumerateTimbermeshCandidateFiles(string folder)
    {
        return Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
            .Where(static path =>
                path.EndsWith(".timbermesh", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase));
    }

    static string Require(string? value, string label)
    {
        while (string.IsNullOrWhiteSpace(value))
        {
            Console.Write($"{label}: ");
            value = Console.ReadLine();
        }

        return value.Trim();
    }

    static string RequireExistingDirectory(string? value, string label)
    {
        while (true)
        {
            value = Require(value, label);

            if (Directory.Exists(value))
            {
                return Path.GetFullPath(value);
            }

            Console.Error.WriteLine($"Directory does not exist: {value}");
            value = null;
        }
    }

    static string RequireExistingPath(string? value, string label)
    {
        while (true)
        {
            value = Require(value, label);

            if (File.Exists(value) || Directory.Exists(value))
            {
                return Path.GetFullPath(value);
            }

            Console.Error.WriteLine($"Path does not exist: {value}");
            value = null;
        }
    }
}

public record ScriptInput(
    string InputFolder,
    string ResourcesFolder,
    string OutputFolder,
    bool Flatten
);
