using System.Text.Json;

string[] filePaths = [
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\MapEditor\Objects\Slope\Slope.Model.timbermesh",
];

var materialMaps = new KeyValuePair<string, string>[]
{
    new("SlopeCliff", "Mud"),
    new("SlopeSand", "Mud"),
    new("SlopeCliffEdge", "DirtUnderground"),
}.ToFrozenDictionary();

const string OutputFolder = @"D:\Temp\Models";
Directory.CreateDirectory(OutputFolder);

HashSet<string> allMaterials = [];
foreach (var path in filePaths)
{
    var model = TimbermeshReader.ParseFile(path);
    Console.WriteLine($"> Model: {model.Name}");

    foreach (var n in model.Nodes)
    {
        Console.WriteLine($"\t> Node {n.Name}");

        var counter = 0;
        foreach (var m in n.Meshes)
        {
            Console.WriteLine($"\t\t> Mesh {++counter}: {m.Material}");
            allMaterials.Add(m.Material);

            if (materialMaps.TryGetValue(m.Material, out var replacement))
            {
                m.Material = replacement;
                Console.WriteLine($"\t\t>> Replaced with: {replacement}");
            }
        }
    }

    var outputName = Path.GetFileNameWithoutExtension(path)
        .Replace("Slope", "Terrace");
        //.Replace("IronTeeth", "Folktails");
    var outputFilePath = Path.Combine(OutputFolder, $"{outputName}.timbermesh");

    await using var stream = File.Create(outputFilePath);
    await TimbermeshReader.WriteToStreamAsync(model, stream);
}

Console.WriteLine("=======");
Console.WriteLine("All materials used:");
Console.WriteLine(JsonSerializer.Serialize(allMaterials, new JsonSerializerOptions { WriteIndented = true }));