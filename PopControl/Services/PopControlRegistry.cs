namespace PopControl.Services;

public class PopControlRegistry(ISingletonLoader loader) : ILoadableSingleton, ISaveableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(PopControl));
    static readonly PropertyKey<string> RegistryDataKey = new("RegistryData");

    PopulationControlData data = null!;

    public DistrictPopulationControl Global => data.Global;

    public DistrictPopulationControl GetControlFor(Guid districtId)
    {
        if (!data.Districts.TryGetValue(districtId, out var districtData))
        {
            data.Districts[districtId] = districtData = new(districtId);
        }

        return districtData;
    }
    public DistrictPopulationControl GetControlFor(DistrictCenter districtCenter)
    {
        var entity = districtCenter.GetComponentFast<EntityComponent>();
        var id = entity.EntityId;

        var data = GetControlFor(id);
        data.DistrictCenter ??= districtCenter;

        return data;
    }

    public void Load()
    {
        LoadSavedData();
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)
            || !s.Has(RegistryDataKey))
        {
            data = new();
            return;
        }

        var json = s.Get(RegistryDataKey);
        data = JsonConvert.DeserializeObject<PopulationControlData>(json)
            ?? new();
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);
        s.Set(RegistryDataKey, JsonConvert.SerializeObject(data));
    }
}

public class PopulationControlData
{
    public DistrictPopulationControl Global { get; set; } = new(Guid.Empty);
    public Dictionary<Guid, DistrictPopulationControl> Districts { get; set; } = [];
}

public record DistrictPopulationControl(Guid DistrictCenterId)
{

    [JsonIgnore]
    public DistrictCenter? DistrictCenter { get; set; }

    public bool LimitBeavers { get; set; }
    public bool LimitBots { get; set; }

    public int Beavers { get; set; }
    public int Bots { get; set; }

}