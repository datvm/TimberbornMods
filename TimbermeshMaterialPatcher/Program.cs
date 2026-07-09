using System.Text.Json;

string[] filePaths = [
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway000010.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway000011.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway001000.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway001001.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway001010.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway001011.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway001100.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway001101.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway001110.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway001111.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway011100.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway011101.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway011110.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway011111.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway101000.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway101001.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway101010.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway101011.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway111100.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway111101.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway111110.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway111111.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubewayCover.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway.IronTeeth.ConstructionStage0.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway000000.IronTeeth.Model.prefab",
@"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Paths\VerticalTubeway\VerticalTubeway000001.IronTeeth.Model.prefab",
];

var materialMaps = new KeyValuePair<string, string>[]
{
    new("WindowsAtlas.IronTeeth", "WindowsAtlas.IronTeeth"),
    new("BaseWood_Indigo.IronTeeth", "BaseWood_Grey.IronTeeth"),
    new("BaseMetal.IronTeeth", "IrregularPlanks_Grey.IronTeeth"),
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

    var outputName = Path.GetFileNameWithoutExtension(path)
        .Replace("VerticalTubeway", "ConveyorBelt");
        //.Replace("IronTeeth", "Folktails");
    var outputFilePath = Path.Combine(OutputFolder, $"{outputName}.timbermesh");

    await using var stream = File.Create(outputFilePath);
    await TimbermeshReader.WriteToStreamAsync(model, stream);
}

Console.WriteLine("=======");
Console.WriteLine("All materials used:");
Console.WriteLine(JsonSerializer.Serialize(allMaterials, new JsonSerializerOptions { WriteIndented = true }));