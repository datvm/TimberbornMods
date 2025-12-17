namespace ModdableWeathers.Weathers.Settings;

public class ModdableWeatherSettingsService(
    IEnumerable<IModdableWeatherSettings> settings,
    ISingletonLoader loader,
    PersistentGameModeService persistentGameModeService
) : BaseWeatherSettingsService<IModdableWeatherSettings>(settings, loader)
{
    protected override SingletonKey SaveKey { get; } = new(nameof(ModdableWeatherSettingsService));

    protected override void InitializeNewData()
    {
        GameModeSpec[] difficulties = [
            persistentGameModeService.ReconstructedMode,
            persistentGameModeService.BestMatchedMode,
            persistentGameModeService.Default];

        foreach (var s in settingsByType.Values)
        {
            foreach (var d in difficulties)
            {
                if (s.CanSupport(d))
                {
                    s.SetTo(d);
                    break;
                }
            }
        }
    }

}
