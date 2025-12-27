namespace ModdableWeathers.Services;

public class ModdableWaterStrengthModifierService(IDayNightCycle dayNightCycle, ISingletonLoader loader) 
    : ModdableWeatherPriorityTickModifierServiceBase<WeatherWaterStrengthModifierEntry>(dayNightCycle, loader)
{
    public static readonly WeatherWaterStrengthModifierEntry DefaultWaterStrengthEntry
        = new(IWeatherEntityModifierEntry.DefaultId, 1, 3, int.MinValue, false);

    public override WeatherWaterStrengthModifierEntry Default { get; } = DefaultWaterStrengthEntry;

    public float CurrentModifierIgnoreDrought => ActiveEntry.IsDrought ? DefaultWaterStrengthEntry.Target : CurrentModifier;

    static readonly SingletonKey StaticSaveKey = new(nameof(ModdableWaterStrengthModifierService));
    protected override SingletonKey SaveKey { get; } = StaticSaveKey;
}

public record WeatherWaterStrengthModifierEntry(string Id, float Target, float Hours, int Priority, bool IsDrought)
    : DefaultWeatherEntityModifierEntry(Id, Target, Hours, Priority);