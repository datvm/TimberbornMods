namespace ModdableTimberborn.Registry;

public abstract class BaseModdableTimberbornConfiguration : IModdableTimberbornRegistryConfig, IModStarter
{
    public static readonly ModdableTimberbornRegistry Registry = ModdableTimberbornRegistry.Instance;

    public abstract ConfigurationContext AvailableContexts { get; }

    public virtual void StartMod(IModEnvironment modEnvironment)
    {
        Registry.AddConfigurator(this);

        if (this is IWithDIConfig)
        {
            Registry.InternalUseDependencyInjection();
        }
    }

    public abstract void Configure(Configurator configurator, ConfigurationContext context);
}

public abstract class BaseModdableTimberbornConfigurationWithHarmony : BaseModdableTimberbornConfiguration, IModdableTimberbornRegistryWithPatchConfig
{
    public virtual string? PatchCategory { get; } = null;
}

public interface IWithDIConfig { }
