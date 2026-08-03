#if DEBUG
args = [
    "-i", @"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Wood",
    "-r", @"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources",
    "-o", @"D:\Temp\TimbermeshBlender",
    "-f",
];
#endif

try
{
    var input = InputService.GetInput(args);

    Console.WriteLine($"Input:    {input.InputFolder}");
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

    await foreach (var timbermeshFile in InputService.GetTimbermeshFilesAsync(input.InputFolder))
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

        var outputPath = BlenderExportService.GetOutputPath(input.InputFolder, input.OutputFolder, timbermeshFile, input.Flatten);
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
catch (Exception ex)
{
    Console.WriteLine("An error occurred while processing:");
    Console.WriteLine(ex.ToString());
}
