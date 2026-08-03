namespace BlueprintReader.Providers;

public class BlueprintProvider
{
    public string GameResourcesFolder { get; }
    readonly FrozenDictionary<string, Type> blueprintSpecTypes;
    readonly FrozenSet<Type> collectionSpecTypes;

    public ImmutableArray<ScriptBlueprint> Blueprints { get; private set; }
    public FrozenDictionary<string, ScriptBlueprint> BlueprintByPath { get; private set; } = null!;
    public FrozenDictionary<Type, ImmutableArray<ScriptBlueprint>> BlueprintsBySpecType { get; private set; } = null!;
    public FrozenDictionary<Type, AggregatedCollectionBlueprint> AggregatedCollections { get; private set; } = null!;

    BlueprintProvider(string gameResourcesFolder, FrozenDictionary<string, Type> blueprintSpecTypes)
    {
        GameResourcesFolder = gameResourcesFolder;
        this.blueprintSpecTypes = blueprintSpecTypes;
        collectionSpecTypes = [.. blueprintSpecTypes.Values.Where(static t => typeof(ICollectionSpec).IsAssignableFrom(t))];
    }

    public IEnumerable<T> GetSpecs<T>() where T : class, IBlueprintSpec => BlueprintsBySpecType[typeof(T)].Select(b => b.GetSpec<T>()!);

    public IEnumerable<object> GetSpecs(Type type) => BlueprintsBySpecType[type].Select(b => b.GetSpec(type));

    public static async Task<BlueprintProvider> CreateAsync(
        string resourcesFolder,
        IEnumerable<Assembly>? additionalSpecAssemblies = null)
    {
        var specTypes = DiscoverSpecTypes(additionalSpecAssemblies);
        BlueprintProvider p = new(resourcesFolder, specTypes);

        var files = Directory.EnumerateFiles(resourcesFolder, "*.blueprint.json", SearchOption.AllDirectories)
            .Select((path, i) => (path, i))
            .ToArray();
        var blueprints = new ScriptBlueprint[files.Length];
        await Parallel.ForEachAsync(files, async (file, _) =>
        {
            blueprints[file.i] = await p.ReadBlueprintAsync(file.path);
        });

        p.Blueprints = [.. blueprints];
        p.BlueprintByPath = p.Blueprints.ToFrozenDictionary(b => b.Path);
        p.AggregateTypes();
        p.AggregateCollections();

        return p;
    }

    static FrozenDictionary<string, Type> DiscoverSpecTypes(IEnumerable<Assembly>? additionalSpecAssemblies)
    {
        HashSet<Assembly> assemblies = [typeof(BlueprintProvider).Assembly];

        if (Assembly.GetEntryAssembly() is { } entryAssembly)
        {
            assemblies.Add(entryAssembly);
        }

        if (additionalSpecAssemblies is not null)
        {
            foreach (var assembly in additionalSpecAssemblies)
            {
                assemblies.Add(assembly);
            }
        }

        return assemblies
            .SelectMany(static a => a.GetTypes())
            .Where(static t => typeof(IBlueprintSpec).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .GroupBy(static t => t.Name)
            .ToFrozenDictionary(static g => g.Key, static g => g.First());
    }

    async Task<ScriptBlueprint> ReadBlueprintAsync(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path)[..^".blueprint".Length];

        var json = await File.ReadAllTextAsync(path);
        var obj = JsonSerializer.Deserialize<JsonElement>(json);

        Dictionary<string, IBlueprintSpec> specs = [];

        foreach (var prop in obj.EnumerateObject())
        {
            var propName = prop.Name;
            if (!blueprintSpecTypes.TryGetValue(propName, out var type))
            {
                continue;
            }

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

        foreach (var t in collectionSpecTypes)
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
