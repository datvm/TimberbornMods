namespace ModdableTimberborn.Registry;

public interface IModdableTimberbornRegistryConfig
{

    void Configure(Configurator configurator, ConfigurationContext context);

}

public interface IModdableTimberbornRegistryWithPatchConfig : IModdableTimberbornRegistryConfig
{
    string PatchCategory { get; }
}