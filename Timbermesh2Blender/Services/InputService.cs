namespace Timbermesh2Blender.Services;

public static class InputService
{
    public static async Task<int> InvokeAsync(string[] args)
    {
        var root = BuildRootCommand();
        return await root.Parse(args).InvokeAsync();
    }

    static RootCommand BuildRootCommand()
    {
        var root = new RootCommand(
            "Timbermesh tools: convert to GLB for Blender, or dump/pack machine-friendly JSON for AI editing");

        // Root options keep backward compatibility: `tmesh2glb -i ... -r ... -o ...`
        var rootGlb = CreateGlbOptions();
        root.Options.Add(rootGlb.Input);
        root.Options.Add(rootGlb.Resources);
        root.Options.Add(rootGlb.Output);
        root.Options.Add(rootGlb.Flatten);
        root.SetAction(async parseResult =>
        {
            if (parseResult.Tokens.Count == 0)
            {
                Console.Error.WriteLine("Specify a command: glb | dump | pack");
                Console.Error.WriteLine("Run with --help for usage.");
                return 1;
            }

            await RunGlbAsync(ReadGlbInput(parseResult, rootGlb));
            return 0;
        });

        root.Subcommands.Add(BuildGlbCommand());
        root.Subcommands.Add(BuildDumpCommand());
        root.Subcommands.Add(BuildPackCommand());
        return root;
    }

    static Command BuildGlbCommand()
    {
        var options = CreateGlbOptions();
        var command = new Command("glb", "Convert Timbermesh models to .glb for Blender")
        {
            options.Input,
            options.Resources,
            options.Output,
            options.Flatten,
        };

        command.SetAction(async parseResult =>
        {
            await RunGlbAsync(ReadGlbInput(parseResult, options));
            return 0;
        });

        return command;
    }

    static Command BuildDumpCommand()
    {
        var inputOption = CreateInputOption("Timbermesh file or folder (.timbermesh / model .prefab)");
        var outputOption = CreateOutputOption("Folder to write .timbermesh.json files");
        var flattenOption = CreateFlattenOption("Write all JSON files directly into the output folder");
        var decodeOption = new Option<bool>("--decode-vertices")
        {
            Description = "Decode float vertex attributes to number arrays (larger files; better for geometry inspection)",
        };

        var command = new Command("dump", "Export Timbermesh to machine-friendly JSON for AI / text editing")
        {
            inputOption,
            outputOption,
            flattenOption,
            decodeOption,
        };

        command.SetAction(async parseResult =>
        {
            var input = RequireExistingPath(parseResult.GetValue(inputOption)?.FullName, "Input path (file or folder)");
            var output = Require(parseResult.GetValue(outputOption), "Output folder");
            var flatten = parseResult.GetValue(flattenOption);
            var decodeVertices = parseResult.GetValue(decodeOption);
            await RunDumpAsync(new DumpInput(input, output, flatten, decodeVertices));
            return 0;
        });

        return command;
    }

    static Command BuildPackCommand()
    {
        var inputOption = CreateInputOption("Timbermesh JSON file or folder (.timbermesh.json)");
        var outputOption = CreateOutputOption("Folder to write .timbermesh files");
        var flattenOption = CreateFlattenOption("Write all .timbermesh files directly into the output folder");

        var command = new Command("pack", "Pack machine-friendly JSON back into .timbermesh")
        {
            inputOption,
            outputOption,
            flattenOption,
        };

        command.SetAction(async parseResult =>
        {
            var input = RequireExistingPath(parseResult.GetValue(inputOption)?.FullName, "Input path (file or folder)");
            var output = Require(parseResult.GetValue(outputOption), "Output folder");
            var flatten = parseResult.GetValue(flattenOption);
            await RunPackAsync(new PackInput(input, output, flatten));
            return 0;
        });

        return command;
    }

    static GlbOptions CreateGlbOptions() => new(
        CreateInputOption("Timbermesh file or folder containing timbermesh models"),
        CreateResourcesOption(),
        CreateOutputOption("Folder to write .glb files"),
        CreateFlattenOption("Write all .glb files directly into the output folder"));

    static GlbInput ReadGlbInput(ParseResult parseResult, GlbOptions options)
    {
        var inputPath = RequireExistingPath(parseResult.GetValue(options.Input)?.FullName, "Input path (file or folder)");
        var textureFolder = RequireExistingDirectory(parseResult.GetValue(options.Resources)?.FullName, "Resources folder");
        var outputFolder = Require(parseResult.GetValue(options.Output), "Output folder");
        var flatten = parseResult.GetValue(options.Flatten);
        return new GlbInput(inputPath, textureFolder, outputFolder, flatten);
    }

    static Option<FileSystemInfo> CreateInputOption(string description)
    {
        var option = new Option<FileSystemInfo>("-i", "--input")
        {
            Description = description,
        };
        option.AcceptExistingOnly();
        return option;
    }

    static Option<DirectoryInfo> CreateResourcesOption()
    {
        var option = new Option<DirectoryInfo>("-r", "--resources")
        {
            Description = "Resources folder containing textures / game assets",
        };
        option.AcceptExistingOnly();
        return option;
    }

    static Option<string> CreateOutputOption(string description) => new("-o", "--output")
    {
        Description = description,
    };

    static Option<bool> CreateFlattenOption(string description) => new("-f", "--flatten")
    {
        Description = description,
    };

    public static async Task RunGlbAsync(GlbInput input)
    {
        Console.WriteLine($"Mode:     glb");
        Console.WriteLine($"Input:    {input.InputPath}");
        Console.WriteLine($"Textures: {input.ResourcesFolder}");
        Console.WriteLine($"Output:   {input.OutputFolder}");
        Console.WriteLine($"Flatten:  {input.Flatten}");

        var bpProvider = await BlueprintProvider.CreateAsync(input.ResourcesFolder);
        var textureService = new TextureService(bpProvider);
        var exportService = new BlenderExportService(textureService);
        Console.WriteLine($"Materials indexed: {textureService.MaterialPaths.Count}");

        Directory.CreateDirectory(input.OutputFolder);

        var count = 0;
        var exported = 0;
        HashSet<string> missingMaterials = new(StringComparer.OrdinalIgnoreCase);

        await foreach (var timbermeshFile in GetTimbermeshFilesAsync(input.InputPath))
        {
            count++;
            var nodeCount = timbermeshFile.Model.Nodes.Length;
            var meshCount = timbermeshFile.Model.Nodes.Sum(static n => n.Meshes.Count);
            Console.WriteLine(
                $"[{count}] {timbermeshFile.Name} (nodes: {nodeCount}, meshes: {meshCount})");

            foreach (var materialName in timbermeshFile.Model.Nodes
                .SelectMany(static n => n.Meshes)
                .Select(static m => m.Material)
                .Where(static m => !string.IsNullOrWhiteSpace(m)))
            {
                if (!textureService.TryGetMaterial(materialName, out _))
                {
                    missingMaterials.Add(materialName);
                }
            }

            var outputPath = BlenderExportService.GetOutputPath(
                input.InputPath, input.OutputFolder, timbermeshFile, input.Flatten);
            await exportService.ExportAsync(timbermeshFile, outputPath);
            exported++;
            Console.WriteLine($"    -> {outputPath}");
        }

        Console.WriteLine($"Done. Loaded {count}, exported {exported} glb file(s).");

        if (missingMaterials.Count > 0)
        {
            Console.WriteLine($"Missing materials ({missingMaterials.Count}):");
            foreach (var name in missingMaterials.Order(StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  - {name}");
            }
        }
    }

    public static async Task RunDumpAsync(DumpInput input)
    {
        Console.WriteLine($"Mode:            dump");
        Console.WriteLine($"Input:           {input.InputPath}");
        Console.WriteLine($"Output:          {input.OutputFolder}");
        Console.WriteLine($"Flatten:         {input.Flatten}");
        Console.WriteLine($"Decode vertices: {input.DecodeVertices}");

        Directory.CreateDirectory(input.OutputFolder);

        var count = 0;
        var exported = 0;

        await foreach (var timbermeshFile in GetTimbermeshFilesAsync(input.InputPath))
        {
            count++;
            var document = TimbermeshJsonService.ToDocument(timbermeshFile.Model, input.DecodeVertices);
            if (string.IsNullOrWhiteSpace(document.Name))
            {
                document.Name = timbermeshFile.Name;
            }

            var outputPath = TimbermeshJsonService.GetDumpOutputPath(
                input.InputPath, input.OutputFolder, timbermeshFile.FilePath, input.Flatten);
            await TimbermeshJsonService.WriteDocumentAsync(document, outputPath);
            exported++;
            Console.WriteLine(
                $"[{count}] {timbermeshFile.Name} (nodes: {document.NodeCount}, tris: {document.TotalTriangles})");
            Console.WriteLine($"    -> {outputPath}");
        }

        Console.WriteLine($"Done. Loaded {count}, dumped {exported} JSON file(s).");
    }

    public static async Task RunPackAsync(PackInput input)
    {
        Console.WriteLine($"Mode:     pack");
        Console.WriteLine($"Input:    {input.InputPath}");
        Console.WriteLine($"Output:   {input.OutputFolder}");
        Console.WriteLine($"Flatten:  {input.Flatten}");

        Directory.CreateDirectory(input.OutputFolder);

        var count = 0;
        var packed = 0;

        foreach (var jsonPath in EnumerateJsonFiles(input.InputPath))
        {
            count++;
            try
            {
                var document = await TimbermeshJsonService.ReadDocumentAsync(jsonPath);
                var model = TimbermeshJsonService.ToModel(document);
                var outputPath = TimbermeshJsonService.GetPackOutputPath(
                    input.InputPath, input.OutputFolder, jsonPath, input.Flatten);
                await TimbermeshFileService.WriteAsync(model, outputPath);
                packed++;
                Console.WriteLine(
                    $"[{count}] {Path.GetFileName(jsonPath)} (nodes: {model.Nodes.Length})");
                Console.WriteLine($"    -> {outputPath}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[{count}] Failed: {jsonPath}");
                Console.Error.WriteLine($"    {ex.Message}");
            }
        }

        Console.WriteLine($"Done. Found {count}, packed {packed} timbermesh file(s).");
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

    static IEnumerable<string> EnumerateJsonFiles(string inputPath)
    {
        if (File.Exists(inputPath))
        {
            if (IsTimbermeshJsonPath(inputPath))
            {
                yield return Path.GetFullPath(inputPath);
            }
            else
            {
                Console.Error.WriteLine($"Not a timbermesh JSON file: {inputPath}");
            }

            yield break;
        }

        if (!Directory.Exists(inputPath))
        {
            throw new DirectoryNotFoundException($"Input path does not exist: {inputPath}");
        }

        foreach (var path in Directory.EnumerateFiles(inputPath, "*.*", SearchOption.AllDirectories)
            .Where(IsTimbermeshJsonPath)
            .OrderBy(static p => p, StringComparer.OrdinalIgnoreCase))
        {
            yield return path;
        }
    }

    static bool IsTimbermeshJsonPath(string path)
        => path.EndsWith(".timbermesh.json", StringComparison.OrdinalIgnoreCase)
            || (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase));

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

    record GlbOptions(
        Option<FileSystemInfo> Input,
        Option<DirectoryInfo> Resources,
        Option<string> Output,
        Option<bool> Flatten
    );
}

public record GlbInput(
    string InputPath,
    string ResourcesFolder,
    string OutputFolder,
    bool Flatten
);

public record DumpInput(
    string InputPath,
    string OutputFolder,
    bool Flatten,
    bool DecodeVertices
);

public record PackInput(
    string InputPath,
    string OutputFolder,
    bool Flatten
);

// Backward-compatible alias used by older call sites / docs.
public record ScriptInput(
    string InputFolder,
    string ResourcesFolder,
    string OutputFolder,
    bool Flatten
) : GlbInput(InputFolder, ResourcesFolder, OutputFolder, Flatten);
