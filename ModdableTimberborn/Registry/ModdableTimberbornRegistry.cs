namespace ModdableTimberborn.Registry;

public partial class ModdableTimberbornRegistry
{
    public static readonly ModdableTimberbornRegistry Instance ;

    readonly HashSet<IModdableTimberbornRegistryConfig> configurators = [];
    
    static readonly HashSet<string> PatchedCategories = [];
    static readonly Harmony harmony;

    public ModdableTimberbornRegistry AddConfigurator(IModdableTimberbornRegistryConfig config)
    {
        configurators.Add(config);

        if (config is IModdableTimberbornRegistryWithPatchConfig patchConfig)
        {
            var category = patchConfig.PatchCategory;

            if (category is not null)
            {
                if (PatchedCategories.Add(category))
                {
                    harmony.PatchCategory(category);
                }
            }
            else
            {
                harmony.PatchAll(patchConfig.GetType().Assembly);
            }
        }

        return this;
    }

    public ModdableTimberbornRegistry AddConfigurator<T>()
        where T : IModdableTimberbornRegistryConfig, new()
        => AddConfigurator(new T());

    static ModdableTimberbornRegistry()
    {
        harmony = new(nameof(ModdableTimberborn));
        Instance = new();
    }

    private ModdableTimberbornRegistry()
    {
        harmony.PatchAllUncategorized();

        AddDefaultConfigurators();
    }

    internal void Configure(Configurator configurator, ConfigurationContext context)
    {
        ModdableTimberbornUtils.CurrentContext = context;

        foreach (var config in configurators)
        {
            config.Configure(configurator, context);
        }
    }

    internal void ConfigureStarter()
    {
        harmony.PatchAllUncategorized();
    }

    void AddDefaultConfigurators()
    {
        AddConfigurator<ModdableEntityDescriberConfigurator>();
    }

}
