namespace BenchmarkAndOptimizer.Services;

public class OptimizerSettingController : ILoadableSingleton
{
    const string EnableBmKey = $"{nameof(ModSettings)}.{nameof(BenchmarkAndOptimizer)}.EnableBenchmark";
    const string OptimizerItemsKey = $"{nameof(ModSettings)}.{nameof(BenchmarkAndOptimizer)}.OptimizerItems";

    static readonly ImmutableArray<Type> OptimizableSingletons = [
        typeof(IUpdatableSingleton), typeof(ILateUpdatableSingleton),
        typeof(ITickableSingleton),
    ];


    public static bool EnableBenchmark
    {
        get => PlayerPrefs.GetInt(EnableBmKey) == 1;
        set
        {
            PlayerPrefs.SetInt(EnableBmKey, value ? 1 : 0);
            if (value)
            {
                MStarter.SwitchBenchmarkOn();
            }
        }
    }

    private FrozenDictionary<string, OptimizerItem> OptimizerItems { get; set; } = FrozenDictionary<string, OptimizerItem>.Empty;

    public static ImmutableHashSet<Type> OptimizableTypesLookup { get; private set; } = [];
    public static ImmutableArray<Type> OptimizableTypes { get; private set; } = [];
    public static readonly ImmutableHashSet<Type> WellKnownTypes = [
        typeof(BehaviorManager),
    ];

    public void Load()
    {
        Scan();
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
        if (!PlayerPrefs.HasKey(OptimizerItemsKey)) { return FrozenDictionary<string, OptimizerItem>.Empty; }

        var json = PlayerPrefs.GetString(OptimizerItemsKey);
        return (JsonConvert.DeserializeObject<Dictionary<string, OptimizerItem>>(json)
               ?? []).ToFrozenDictionary();
    }

    void Save()
    {
        PlayerPrefs.SetString(OptimizerItemsKey, JsonConvert.SerializeObject(OptimizerItems));
    }

    static void Scan()
    {
        if (OptimizableTypesLookup.Count > 0) { return; }

        OptimizableTypesLookup = [.. ScanForTypes()];
        OptimizableTypes = [.. OptimizableTypesLookup.OrderBy(q => q.Name)];
    }

    static IEnumerable<Type> ScanForTypes()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            foreach (var t in asm.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface) { continue; }

                if (IsOptimizableComponent(t) || IsOptimizableSingleton(t))
                {
                    yield return t;
                }
            }
        }
    }

    static bool IsOptimizableComponent(Type t) =>
        typeof(BaseComponent).IsAssignableFrom(t)
        && (
            typeof(TickableComponent).IsAssignableFrom(t)
            || t.GetMethod("Update") != null
            || t.GetMethod("LateUpdate") != null
        );

    static bool IsOptimizableSingleton(Type t)
    {
        foreach (var i in OptimizableSingletons)
        {
            if (i.IsAssignableFrom(t))
            {
                return true;
            }
        }

        return false;
    }

}

[method: JsonConstructor]
public record OptimizerItem(string Type)
{
    public OptimizerItem(Type type) : this(type.FullName) { }

    public bool Enabled { get; set; }
    public int Value { get; set; }
}
