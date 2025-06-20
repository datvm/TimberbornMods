namespace FileFinder;

public class ShaderMapper(FileMapper fileMapper)
{
    public const string RootFolder = @"D:\Personal\Mods\Timberborn\U7Data\ExportedProject\Assets\Resources";

    public async Task PrintShaderMappingsAsync()
    {
        var materialFiles = Directory.GetFiles(RootFolder, "*.mat", SearchOption.AllDirectories);

        foreach (var matFile in materialFiles)
        {
            var name = Path.GetFileNameWithoutExtension(matFile);

            using var file = File.OpenRead(matFile);
            using var reader = new StreamReader(file);

            string? line;
            while ((line =await reader.ReadLineAsync()) is not null)
            {
                if (!line.Contains("m_Shader:")) { continue; }

                var guid = Utils.ParseForGuid(line);
                if (guid is null) { continue; }

                if (fileMapper.FilesByGuid.TryGetValue(guid, out var gameFile))
                {
                    var shaderName = gameFile.Name;
                    Console.WriteLine($"{name}: {shaderName}");
                }
                else
                {
                    Console.WriteLine($"Shader not found for {name}");
                }
                break;
            }
        }
    }

}
