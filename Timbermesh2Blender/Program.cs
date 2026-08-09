#if DEBUG
args =
[
    "dump",
    "-i", @"D:\Personal\Mods\Timberborn\V1Data\ExportedProject\Assets\Resources\Buildings\Wood\Forester\Forester.Folktails.Model.timbermesh",
    "-o", @"D:\Temp\TimbermeshModel",
    "-f",
];
#endif

try
{
    return await InputService.InvokeAsync(args);
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred while processing:");
    Console.WriteLine(ex.ToString());
    return 1;
}
