namespace ModdableWeathers.WeatherModifiers;

public class ModdableWeatherModifierRegistry(
    IEnumerable<IModdableWeatherModifier> modifiers
) : ILoadableSingleton
{
    public ImmutableArray<IModdableWeatherModifier> Modifiers { get; private set; } = [];
    public FrozenDictionary<string, IModdableWeatherModifier> ModifiersById { get; private set; } = FrozenDictionary<string, IModdableWeatherModifier>.Empty;

    public void Load()
    {
        Modifiers = [.. modifiers.OrderBy(q => q.Spec.Order)];
        ModifiersById = Modifiers.ToFrozenDictionary(q => q.Id);
    }

    public IModdableWeatherModifier? GetOrFallback(string id)
        => ModifiersById.GetValueOrDefault(id);
}
