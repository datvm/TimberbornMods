namespace ConfigurableFaction.Services;

public class FactionBuildingService(ISpecService specs) : ILoadableSingleton
{
    public static FactionBuildingService? Instance { get; private set; }
    static string InfoFile => Path.Combine(ModStarter.FactionFolder, "Info.json");

    public FrozenDictionary<string, FactionInfo> Factions { get; private set; } = FrozenDictionary<string, FactionInfo>.Empty;

    public void Load()
    {
        var factions = LoadInfoFromFile();
        AppendFactionFromSpecs(factions);

        Factions = factions.ToFrozenDictionary();

        Instance = this;
    }

    void SaveInfo()
    {
        var json = JsonConvert.SerializeObject(Factions);
        File.WriteAllText(InfoFile, json);
    }

    public void PopulateFactionBuildingInfo(string id, List<SimpleBuildingSpec> buildings)
    {
        List<FactionInfo> factions = [.. Factions.Values];
        for (int i = 0; i < factions.Count; i++)
        {
            if (factions[i].Id == id)
            {
                factions[i] = factions[i] with { Buildings = [.. buildings] };
                break;
            }
        }

        CheckForCommonBuildings(factions);
        Factions = factions.ToFrozenDictionary(q => q.Id);

        SaveInfo();
    }

    void CheckForCommonBuildings(List<FactionInfo> factions)
    {
        Dictionary<string, int> counts = [];

        foreach (var f in factions)
        {
            foreach (var b in f.Buildings)
            {
                var id = TryGetCommonId(b.Id);
                counts[id] = counts.GetValueOrDefault(id) + 1;
            }
        }

        var factionsCount = factions.Count;
        for (int i = 0; i < factions.Count; i++)
        {
            factions[i] = factions[i] with
            {
                Buildings = [..factions[i].Buildings
                    .Select(q => q with { IsCommon = counts[TryGetCommonId(q.Id)] == factionsCount })]
            };
        }

        static string TryGetCommonId(string id)
        {
            var lastDot = id.LastIndexOf('.');
            return lastDot == -1 ? id : id[..lastDot];
        }
    }

    Dictionary<string, FactionInfo> AppendFactionFromSpecs(Dictionary<string, FactionInfo> dict)
    {
        var factions = specs.GetSpecs<FactionSpec>();
        foreach (var f in factions)
        {
            if (!dict.ContainsKey(f.Id))
            {
                dict.Add(f.Id, new FactionInfo(f, []));
            }
        }

        return dict;
    }

    Dictionary<string, FactionInfo> LoadInfoFromFile()
    {
        if (!File.Exists(InfoFile))
        {
            return [];
        }

        var json = File.ReadAllText(InfoFile);
        return JsonConvert.DeserializeObject<Dictionary<string, FactionInfo>>(json)!;
    }

}
