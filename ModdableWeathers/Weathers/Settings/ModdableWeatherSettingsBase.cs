namespace ModdableWeathers.Weathers.Settings;

public abstract partial class DefaultModdableWeatherSettings : IDefaultModdableWeatherSettings
{

    public virtual bool CanSupport(GameModeSpec gameMode) => true;
    public abstract void SetTo(GameModeSpec gameMode);

}
