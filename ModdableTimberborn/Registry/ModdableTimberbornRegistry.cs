
namespace ModdableTimberborn.Registry;

public class ModdableTimberbornRegistry
{
    public static readonly ModdableTimberbornRegistry Instance ;

    readonly HashSet<IModdableTimberbornRegistryConfig> configurators = [];
    
    static readonly HashSet<string> PatchedCategories = [];
    static readonly Harmony harmony;


    public bool MechanicalSystemUsed { get; private set; }
    public ModdableTimberbornRegistry UseMechanicalSystem()
    {
        if (MechanicalSystemUsed) { return this; }

        MechanicalSystemUsed = true;
        AddConfigurator(ModdableMechanicalSystemConfigurator.Instance);

        return this;
    }

    public ModdableTimberbornRegistry AddConfigurator(IModdableTimberbornRegistryConfig config)
    {
        configurators.Add(config);

        if (config is IModdableTimberbornRegistryWithPatchConfig patchConfig)
        {
            var category = patchConfig.PatchCategory;
            if (PatchedCategories.Add(category))
            {
                harmony.PatchCategory(category);
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
