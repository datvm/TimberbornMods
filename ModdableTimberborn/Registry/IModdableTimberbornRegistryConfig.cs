namespace ModdableTimberborn.Registry;

/// <summary>
/// An interface to configure services in different contexts
/// </summary>
public interface IModdableTimberbornRegistryConfig
{

    ConfigurationContext AvailableContexts => ConfigurationContext.All;

    void Configure(Configurator configurator, ConfigurationContext context);

}

/// <summary>
/// An interface to configure services in different contexts with optional Harmony patching
/// </summary>
public interface IModdableTimberbornRegistryWithPatchConfig : IModdableTimberbornRegistryConfig
{
    /// <summary>
    /// If specified, only this Harmony patch category will be patched.
    /// If null, all patches in the assembly of this type will be applied.
    /// </summary>
    string? PatchCategory { get; }
}

public interface IWithDIConfig { }