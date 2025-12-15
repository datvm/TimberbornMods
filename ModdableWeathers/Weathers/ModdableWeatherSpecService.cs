namespace ModdableWeathers.Weathers;

public class ModdableWeatherSpecService(ISpecService specs) : ILoadableSingleton
{

    public ImmutableArray<ModdableWeatherSpec> AllSpecs { get; private set; } = [];
    public FrozenDictionary<string, ModdableWeatherSpec> SpecsById { get; private set; } = FrozenDictionary<string, ModdableWeatherSpec>.Empty;

    public void Load()
    {
        AllSpecs = [.. specs.GetSpecs<ModdableWeatherSpec>()];
        SpecsById = AllSpecs.ToFrozenDictionary(spec => spec.Id);
    }

}
