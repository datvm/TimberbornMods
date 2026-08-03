#if DEBUG
args = [
    "-i", @"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Wood",
    "-textures", @"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources",
    "-o", @"D:\Temp\TimbermeshBlender",
];
#endif

try
{
    var services = CreateServices();
    var input = InputService.GetInput(args);

    Console.WriteLine($"Input:    {input.InputFolder}");
    Console.WriteLine($"Textures: {input.TextureFolder}");
    Console.WriteLine($"Output:   {input.OutputFolder}");

    var count = 0;
    await foreach (var timbermeshFile in InputService.GetTimbermeshFilesAsync(input.InputFolder))
    {
        count++;
        var nodeCount = timbermeshFile.Model.Nodes.Length;
        var meshCount = timbermeshFile.Model.Nodes.Sum(static n => n.Meshes.Count);
        Console.WriteLine(
            $"Loaded {timbermeshFile.Name} (nodes: {nodeCount}, meshes: {meshCount}) from {timbermeshFile.FilePath}");
        // Will process later
    }

    Console.WriteLine($"Done. Loaded {count} timbermesh file(s).");
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred while processing:");
    Console.WriteLine(ex.ToString());
}

static IServiceProvider CreateServices() => new ServiceCollection()
    .AddServiceSharp()
    .BuildServiceProvider();
