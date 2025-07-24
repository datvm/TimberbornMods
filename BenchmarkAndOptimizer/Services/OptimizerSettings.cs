namespace BenchmarkAndOptimizer.Services;

public class OptimizerSettings(ISystemFileDialogService diag) : ILoadableSingleton
{
    const string EnableBmKey = $"{nameof(ModSettings)}.{nameof(BenchmarkAndOptimizer)}.EnableBenchmark";
    const string OptimizerItemsKey = $"{nameof(ModSettings)}.{nameof(BenchmarkAndOptimizer)}.OptimizerItems";

    public static bool EnableBenchmark
    {
        get => PlayerPrefs.GetInt(EnableBmKey) == 1;
        set
        {
            PlayerPrefs.SetInt(EnableBmKey, value ? 1 : 0);
        }
    }

    private FrozenDictionary<string, OptimizerItem> OptimizerItems { get; set; } = FrozenDictionary<string, OptimizerItem>.Empty;

    public void Load()
    {
        OptimizerItems = LoadItems();
    }

    public void SetOptimizer(OptimizerItem value)
    {
        if (OptimizerItems.TryGetValue(value.Type, out var existingValue) && existingValue == value)
        {
            return; // No change needed
        }

        var dict = OptimizerItems.ToDictionary(q => q.Key, q => q.Value);
        dict[value.Type] = value with { };
        OptimizerItems = dict.ToFrozenDictionary();

        Save();
    }

    public void RemoveOptimizer(string type)
    {
        if (!OptimizerItems.ContainsKey(type)) { return; }

        var dict = OptimizerItems.ToDictionary(q => q.Key, q => q.Value);
        dict.Remove(type);
        OptimizerItems = dict.ToFrozenDictionary();

        Save();
    }

    public void Clear()
    {
        OptimizerItems = FrozenDictionary<string, OptimizerItem>.Empty;
        PlayerPrefs.DeleteKey(OptimizerItemsKey);
    }

    public void Export()
    {
        var file = diag.ShowSaveFileDialog(".json");
        if (file is null) { return; }

        File.WriteAllText(file, SerializedSettings);
    }

    public void Import()
    {
        var file = diag.ShowOpenFileDialog(".json");
        if (file is null) { return; }

        var json = File.ReadAllText(file);
        var items = LoadItems(json);

        OptimizerItems = items;
    }

    public OptimizerItem? GetValue(Type type)
    {
        if (OptimizerItems.TryGetValue(type.FullName, out var value))
        {
            return value;
        }
        return null;
    }

    static FrozenDictionary<string, OptimizerItem> LoadItems()
    {
        var json = PlayerPrefs.HasKey(OptimizerItemsKey) ? PlayerPrefs.GetString(OptimizerItemsKey) : null;
        return LoadItems(json);
    }

    static FrozenDictionary<string, OptimizerItem> LoadItems(string? json)
    {
        return string.IsNullOrWhiteSpace(json)
            ? FrozenDictionary<string, OptimizerItem>.Empty
            : (JsonConvert.DeserializeObject<Dictionary<string, OptimizerItem>>(json)
               ?? []).ToFrozenDictionary();
    }

    string SerializedSettings => JsonConvert.SerializeObject(OptimizerItems);

    void Save()
    {
        PlayerPrefs.SetString(OptimizerItemsKey, SerializedSettings);
    }

}

[method: JsonConstructor]
public record OptimizerItem(string Type)
{
    public OptimizerItem(Type type) : this(type.FullName) { }

    public bool Enabled { get; set; }
    public int Value { get; set; }
}
