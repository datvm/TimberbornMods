namespace ModdableTimberborn.Registry;

/// <summary>
/// The context in which the configuration is being applied.
/// </summary>
[Flags]
public enum ConfigurationContext
{
    Bootstrapper = 0b0001,
    MainMenu = 0b0010,
    Game = 0b0100,
    MapEditor = 0b1000,

    All = 0b1111,
    None = 0,
}