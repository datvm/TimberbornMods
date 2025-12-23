namespace ModdableWeathers.Services;

/// <summary>
/// Modify water contamination levels. The water sources' contamination levels
/// add directly with this modifier (-1 = completely clean, 0 = no change, 1 = completely contaminated).
/// </summary>
public class ModdableWaterContaminationModifierService(IDayNightCycle dayNightCycle)
    : ModdableWeatherPriorityTickModifierServiceBase<DefaultWeatherEntityModifierEntry>(dayNightCycle)
{
    static readonly DefaultWeatherEntityModifierEntry DefaultContaminationEntry 
        = new(IWeatherEntityModifierEntry.DefaultId, 0, 3, int.MinValue);
    public override DefaultWeatherEntityModifierEntry Default { get; } = DefaultContaminationEntry;

}
