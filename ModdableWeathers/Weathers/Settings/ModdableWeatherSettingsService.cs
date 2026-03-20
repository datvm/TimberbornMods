namespace ModdableWeathers.Weathers.Settings;

public class ModdableWeatherSettingsService(
    IEnumerable<IModdableWeatherSettings> settings,
    ISingletonLoader loader,
    PersistentGameModeService persistentGameModeService
) : BaseWeatherSettingsService<IModdableWeatherSettings>(settings, loader)
{
    protected override SingletonKey SaveKey { get; } = new(nameof(ModdableWeatherSettingsService));

    public override void Load()
    {
        base.Load();
        if (!HasNewSettingEntry) { return; }

        GameModeSpec[] difficulties = [
            persistentGameModeService.ReconstructedMode,
            persistentGameModeService.BestMatchedMode,
            persistentGameModeService.Default];

        foreach (var s in settingsByType.Values)
        {
            if (!s.FirstLoad) { continue; }

            foreach (var d in difficulties)
            {
                if (s.CanSupport(d))
                {
                    s.InitializeNewValuesTo(d);
                    break;
                }
            }
        }
    }

}
