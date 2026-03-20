namespace SkyblightWeather;

public class SkyblightWeatherConfigs : BaseModdableTimberbornAttributeConfiguration, IModdableTimberbornRegistryWithPatchConfig
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;
    public string? PatchCategory => null;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseEntityTracker()
            .TryTrack<BadrainCharacterComponent>();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        base.Configure(configurator, context);

        configurator
            .BindWeather<SkyblightModdableWeather, SkyblightWeatherSettings>()
            .BindWeatherModifier<SkyblightModifier, SkyblightModifierSettings>()
            .BindWeatherModifier<BadrainModifier, BadrainModifierSettings>()
        ;
    }
}
