namespace ModdableTimberborn.Registry;

public abstract class BaseModdableTimberbornConfiguration : IModdableTimberbornRegistryConfig, IModStarter
{
    public virtual void StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance.AddConfigurator(this);
    }

    public abstract void Configure(Configurator configurator, ConfigurationContext context);
}

public abstract class BaseModdableTimberbornConfigurationWithHarmony : BaseModdableTimberbornConfiguration, IModdableTimberbornRegistryWithPatchConfig
{
    public virtual string? PatchCategory { get; } = null;
}
