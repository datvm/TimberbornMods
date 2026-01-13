namespace ModdableWeathers.Services;

public class GeneralGlobalSettings : IBaseWeatherSettings
{
    const string SettingPrefix = $"{nameof(ModdableWeathers)}.General.";
    const string DoNotShowOnNewGameKey = $"{SettingPrefix}DoNotShowOnNewGame";

    [Description("LV.MW.NoNewSetting")]
    public bool DoNotShowOnNewGame
    {
        get => PlayerPrefs.GetInt(DoNotShowOnNewGameKey, 0) == 1;
        set => PlayerPrefs.SetInt(DoNotShowOnNewGameKey, value ? 1 : 0);
    }

}
