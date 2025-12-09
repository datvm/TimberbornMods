global using Newtonsoft.Json;

namespace ToolHotkey.Services;

public readonly record struct AdditionalToolSpec(string Id, string Name, string? Group);

public static class ToolPersistence
{
    const int CurrentVersion = 710;

    public static string ToolsPath => Path.Combine(ModStarter.ModFolder, "tools.json");

    public static void SaveTools(IEnumerable<AdditionalToolSpec> tools)
    {
        var data = new ToolPersistenceData
        {
            Version = CurrentVersion,
            Tools = [.. tools],
        };

        var json = JsonConvert.SerializeObject(data);
        File.WriteAllText(ToolsPath, json);
    }

    public static List<AdditionalToolSpec> LoadTools()
    {
        if (!File.Exists(ToolsPath)) { return []; }

        try
        {
            var json = File.ReadAllText(ToolsPath);
            var data = JsonConvert.DeserializeObject<ToolPersistenceData>(json)!;

            return data.Version < CurrentVersion ? [] : data.Tools;
        }
        catch (Exception)
        {
            return [];
        }        
    }

    class ToolPersistenceData
    {
        public int Version { get; set; }
        public List<AdditionalToolSpec> Tools { get; set; } = [];
    }

}
