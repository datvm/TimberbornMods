namespace ModdableWeather.Services.Registries;

public class WeatherModifierRegistry(
    IEnumerable<WeatherModifier> allModifiers
    ISpecService specs
) : ILoadableSingleton
{

    public FrozenDictionary<string, WeatherModifierSpec> Specs { get; private set; } = FrozenDictionary<string, WeatherModifierSpec>.Empty;

    public void Load()
    {
        var allSpecs = specs.GetSpecs<WeatherModifierSpec>();
        Specs = allSpecs.ToFrozenDictionary(q => q.Id);
    }

}
