namespace ModdableWeathers.Weathers.Settings;

public abstract partial class DefaultModdableWeatherSettings : IDefaultModdableWeatherSettings
{

    public bool FirstLoad { get; set; }

    public virtual bool CanSupport(GameModeSpec gameMode) => true;
    public abstract void InitializeNewValuesTo(GameModeSpec gameMode);

}
