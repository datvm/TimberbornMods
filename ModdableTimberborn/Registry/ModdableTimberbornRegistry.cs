namespace ModdableTimberborn.Registry;

public partial class ModdableTimberbornRegistry
{
    public static readonly ImmutableArray<ConfigurationContext> SingleContexts = [ConfigurationContext.Bootstrapper, ConfigurationContext.MainMenu, ConfigurationContext.Game, ConfigurationContext.MapEditor];

    public static readonly ModdableTimberbornRegistry Instance;

    readonly HashSet<IModdableTimberbornRegistryConfig> configurators = [];
    readonly Dictionary<ConfigurationContext, List<IModdableTimberbornRegistryConfig>> configuratorsByContext = [];

    static readonly HashSet<string> PatchedCategories = [];
    static readonly Harmony harmony;

    public ModdableTimberbornRegistry AddConfigurator(IModdableTimberbornRegistryConfig config)
    {
        configurators.Add(config);

        var availableCtx = config.AvailableContexts;
        foreach (var ctx in configuratorsByContext.Keys)
        {
            if (availableCtx.HasFlag(ctx))
            {
                configuratorsByContext[ctx].Add(config);
            }
        }

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

    ModdableTimberbornRegistry()
    {
        harmony.PatchAllUncategorized();


        foreach (var ctx in SingleContexts)
        {
            configuratorsByContext[ctx] = [];
        }
        AddDefaultConfigurators();
    }

    internal void Configure(Configurator configurator, ConfigurationContext context)
    {
        ModdableTimberbornUtils.CurrentContext = context;

        foreach (var config in configuratorsByContext[context])
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
        AddConfigurator<ModdableTimberbornConfigurator>();
    }

}
