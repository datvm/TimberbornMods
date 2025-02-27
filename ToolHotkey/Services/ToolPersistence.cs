global using Newtonsoft.Json;

namespace ToolHotkey.Services;

public readonly record struct AdditionalToolSpec(string Id, string GroupId, string NameLoc, string TName);

public static class ToolPersistence
{

    public static string ToolsPath => Path.Combine(ModStarter.ModFolder, "tools.json");

    public static void SaveTools(IEnumerable<AdditionalToolSpec> tools)
    {
        var json = JsonConvert.SerializeObject(tools);
        File.WriteAllText(ToolsPath, json);
    }

    public static List<AdditionalToolSpec> LoadTools()
    {
        if (!File.Exists(ToolsPath)) { return []; }

        var json = File.ReadAllText(ToolsPath);
        return JsonConvert.DeserializeObject<List<AdditionalToolSpec>>(json)!;
    }

}
