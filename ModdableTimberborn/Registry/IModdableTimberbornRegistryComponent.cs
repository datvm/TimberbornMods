namespace ModdableTimberborn.Registry;

public interface IModdableTimberbornRegistryComponent
{

    void Configure(Configurator configurator, ConfigurationContext context);

}

public interface IModdableTimberbornRegistryWithPatchComponent : IModdableTimberbornRegistryComponent
{
    string PatchCategory { get; }
}