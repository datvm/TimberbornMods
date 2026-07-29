namespace TechTreeBlueprintScript.Providers;

public class BlueprintProvider
{
    static readonly FrozenDictionary<string, Type> BlueprintSpecTypes = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => typeof(IBlueprintSpec).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
        .ToFrozenDictionary(t => t.Name);
    static readonly FrozenSet<Type> CollectionSpecTypes = [.. BlueprintSpecTypes.Values.Where(t => typeof(ICollectionSpec).IsAssignableFrom(t))];

    const string GameResourcesFolder = @"D:/Personal/Mods/Timberborn/V1Data/ExportedProject/Assets/Resources";

    public ImmutableArray<ScriptBlueprint> Blueprints { get; private set; }
    public FrozenDictionary<string, ScriptBlueprint> BlueprintByPath { get; private set; } = null!;
    public FrozenDictionary<Type, ImmutableArray<ScriptBlueprint>> BlueprintsBySpecType { get; private set; } = null!;
    public FrozenDictionary<Type, AggregatedCollectionBlueprint> AggregatedCollections { get; private set; } = null!;

    BlueprintProvider() { }

    public IEnumerable<T> GetSpecs<T>() where T : class, IBlueprintSpec => BlueprintsBySpecType[typeof(T)].Select(b => b.GetSpec<T>()!);

    public IEnumerable<object> GetSpecs(Type type) => BlueprintsBySpecType[type].Select(b => b.GetSpec(type));

    public static async Task<BlueprintProvider> CreateAsync()
    {
        BlueprintProvider p = new();

        var files = Directory.EnumerateFiles(GameResourcesFolder, "*.blueprint.json", SearchOption.AllDirectories)
            .Select((p, i) => (p, i))
            .ToArray();
        var blueprints = new ScriptBlueprint[files.Length];
        await Parallel.ForEachAsync(files, async (file, _) =>
        {
            var blueprint = await ReadBlueprintAsync(file.p);
            blueprints[file.i] = blueprint;
        });

        p.Blueprints = [.. blueprints];
        p.BlueprintByPath = p.Blueprints.ToFrozenDictionary(b => b.Path);
        p.AggregateTypes();
        p.AggregateCollections();

        return p;
    }

    static async Task<ScriptBlueprint> ReadBlueprintAsync(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path)[..^".blueprint".Length];

        var json = await File.ReadAllTextAsync(path);
        var obj = JsonSerializer.Deserialize<JsonElement>(json);

        Dictionary<string, IBlueprintSpec> specs = [];

        foreach (var prop in obj.EnumerateObject())
        {
            var propName = prop.Name;
            if (!BlueprintSpecTypes.TryGetValue(propName, out var type)) { continue; }

            specs.Add(propName, prop.Value.Deserialize(type) as IBlueprintSpec
                ?? throw new InvalidOperationException("Invalid object"));
        }

        var relativePath = Path.GetRelativePath(GameResourcesFolder, path).Replace('\\', '/')[..^".json".Length];
        return new(name, relativePath, specs.ToFrozenDictionary());
    }

    void AggregateTypes()
    {
        Dictionary<Type, List<ScriptBlueprint>> blueprintsByType = [];

        foreach (var blueprint in Blueprints)
        {
            foreach (var spec in blueprint.Specs.Values)
            {
                var type = spec.GetType();
                if (!blueprintsByType.TryGetValue(type, out var list))
                {
                    blueprintsByType[type] = list = [];
                }

                list.Add(blueprint);
            }
        }

        BlueprintsBySpecType = blueprintsByType.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.ToImmutableArray());
    }

    void AggregateCollections()
    {
        Dictionary<Type, AggregatedCollectionBlueprint> collections = [];

        foreach (var t in CollectionSpecTypes)
        {
            if (GetSpecs(t).Cast<ICollectionSpec>().ToArray() is var bps && bps.Length == 0)
            {
                collections[t] = new AggregatedCollectionBlueprint(t, FrozenDictionary<string, ImmutableArray<string>>.Empty);
                continue;
            }

            Dictionary<string, HashSet<string>> subCollections = [];
            foreach (var bp in bps)
            {
                var id = bp.Id;
                if (!subCollections.TryGetValue(id, out var idCol))
                {
                    idCol = subCollections[id] = [];
                }

                idCol.UnionWith(bp.Collection);
            }

            collections[t] = new(t, subCollections.ToFrozenDictionary(kv => kv.Key, kv => kv.Value.ToImmutableArray()));
        }

        AggregatedCollections = collections.ToFrozenDictionary();
    }

}
