using System.Text.Json;

string[] filePaths = [
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway001010.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway001011.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway001100.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway001101.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway001110.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway001111.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway011100.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway011101.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway011110.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway011111.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway101000.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway101001.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway101010.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway101011.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway111100.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway111101.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway111110.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway111111.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubewayCover.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway.IronTeeth.ConstructionStage0.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway000000.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway000001.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway000010.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway000011.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway001000.IronTeeth.Model.prefab",
    @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources\buildings\paths\verticaltubeway\VerticalTubeway001001.IronTeeth.Model.prefab",
];

var materialMaps = new KeyValuePair<string, string>[]
{
    new("BaseWood_Indigo.IronTeeth", "BaseWood_Grey.IronTeeth"),
    new("BaseWood_DarkBrown.IronTeeth", "BaseWood_Grey.IronTeeth"),
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

    var outputName = Path.GetFileNameWithoutExtension(path).Replace("VerticalTubeway", "ConveyorBelt");
    var outputFilePath = Path.Combine(OutputFolder, $"{outputName}.timbermesh");

    await using var stream = File.Create(outputFilePath);
    await TimbermeshReader.WriteToStreamAsync(model, stream);
}

Console.WriteLine("=======");
Console.WriteLine("All materials used:");
Console.WriteLine(JsonSerializer.Serialize(allMaterials, new JsonSerializerOptions { WriteIndented = true }));