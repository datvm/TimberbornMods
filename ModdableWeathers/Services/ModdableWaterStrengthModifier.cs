namespace ModdableWeathers.Services;

public class ModdableWaterStrengthModifierService(IDayNightCycle dayNightCycle) 
    : ModdableWeatherPriorityTickModifierServiceBase<DefaultWeatherEntityModifierEntry>(dayNightCycle)
{
    public static readonly DefaultWeatherEntityModifierEntry DefaultWaterStrengthEntry
        = new(IWeatherEntityModifierEntry.DefaultId, 1, 3, int.MinValue);

    public override DefaultWeatherEntityModifierEntry Default { get; } = DefaultWaterStrengthEntry;
}