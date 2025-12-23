namespace ModdableWeathers.Common.PrioritizedModifiers;

public interface IWeatherEntityModifierEntry
{
    const string DefaultId = "Default";

    string Id { get; }
    int Priority { get; }
}

public interface IWeatherEntityTickModifierEntry : IWeatherEntityModifierEntry
{
    float Target { get; }
    float Hours { get; }
}

public record DefaultWeatherEntityModifierEntry(string Id, float Target, float Hours, int Priority) : IWeatherEntityTickModifierEntry;