namespace ModdableWeathers.WeatherModifiers;

public class ModdableWeatherModifierSpecService(ISpecService specs) : ILoadableSingleton
{

    public ImmutableArray<ModdableWeatherModifierSpec> AllSpecs { get; private set; } = [];
    public FrozenDictionary<string, ModdableWeatherModifierSpec> SpecsById { get; private set; } = FrozenDictionary<string, ModdableWeatherModifierSpec>.Empty;

    public void Load()
    {
        AllSpecs = [.. specs.GetSpecs<ModdableWeatherModifierSpec>().OrderBy(q => q.Order)];
        SpecsById = AllSpecs.ToFrozenDictionary(spec => spec.Id);
    }

}
