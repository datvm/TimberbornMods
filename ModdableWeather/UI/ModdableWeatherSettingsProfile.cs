namespace ModdableWeather.UI;

public class ModdableWeatherProfileSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(ModdableWeather);
    public override string HeaderLocKey { get; } = "LV.MW.Profiles";
    public override int Order => -1;

    public ModdableWeatherSettingsProfile Profile { get; } = new();

}

public class ModdableWeatherSettingsProfile : NonPersistentSetting
{

    public ModdableWeatherSettingsProfile() : base(new("", "")) { }

}

public class ModdableWeatherSettingsProfileFactory(
    IContainer container
) : IModSettingElementFactory
{
    public int Priority { get; }

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {
        if (modSetting is not ModdableWeatherSettingsProfile)
        {
            element = default;
            return false;
        }

        element = new ModSettingElement(
            container.GetInstance<ModdableWeatherProfileElement>(),
            modSetting);
        return true;
    }

}
