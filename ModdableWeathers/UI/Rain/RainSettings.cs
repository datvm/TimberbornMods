namespace ModdableWeathers.UI.Rain;

public class RainSettings
{
    const string SettingPrefix = $"{nameof(ModdableWeathers)}.Rain.";
    const string EnabledKey = $"{SettingPrefix}Enabled";
    const string IntensityKey = $"{SettingPrefix}Intensity";
    const string MaxParticlesKey = $"{SettingPrefix}MaxParticles";

    public event Action OnSettingsChanged = null!;

    public bool RainEnabled
    {
        get => PlayerPrefs.GetInt(EnabledKey, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
            OnSettingsChanged();
        }
    }

    public float RainIntensity
    {
        get => PlayerPrefs.GetFloat(IntensityKey, .2f);
        set
        {
            PlayerPrefs.SetFloat(IntensityKey, value);
            OnSettingsChanged();
        }
    }

    public int MaxRainParticles
    {
        get => PlayerPrefs.GetInt(MaxParticlesKey, 1000);
        set
        {
            PlayerPrefs.SetInt(MaxParticlesKey, value);
            OnSettingsChanged();
        }
    }
}
