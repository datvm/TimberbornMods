namespace ModdableTimberborn.Registry;

public class ModdableTimberbornRegistry
{
    public static readonly ModdableTimberbornRegistry Instance = new();

    readonly HashSet<IModdableTimberbornRegistryComponent> configurators = [];
    
    static readonly HashSet<string> PatchedCategories = [];
    static readonly Harmony harmony = new(nameof(ModdableTimberborn));

    private ModdableTimberbornRegistry()
    {
        harmony.PatchAllUncategorized();
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

    public bool MechanicalSystemUsed { get; private set; }
    public ModdableTimberbornRegistry UseMechanicalSystem()
    {
        if (MechanicalSystemUsed) { return this; }

        MechanicalSystemUsed = true;
        AddConfigurator(ModdableMechanicalSystemConfigurator.Instance);

        return this;
    }

    public void AddConfigurator(IModdableTimberbornRegistryComponent config)
    {
        configurators.Add(config);

        if (config is IModdableTimberbornRegistryWithPatchComponent patchConfig)
        {
            var category = patchConfig.PatchCategory;
            if (PatchedCategories.Add(category))
            {
                harmony.PatchCategory(category);
            }
        }
    }
}
