namespace ConfigurableFaction.UI;

public class ConfigurableFactionSettings(UserSettingsService userSettingsService) : NonPersistentSetting(new("", ""))
{

    public override void Reset()
    {
        base.Reset();
        userSettingsService.Reset();
    }

}

[MultiBind(typeof(IModSettingElementFactory), Contexts = BindAttributeContext.MainMenu)]
public class ConfigurableFactionSettingsFactory(IContainer container) : IModSettingElementFactory
{
    public int Priority { get; }

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {        
        if (modSetting is not ConfigurableFactionSettings)
        {
            element = null;
            return false;
        }

        element = new ModSettingElement(container.GetInstance<ConfigurableFactionSettingsPanel>(), modSetting);
        return true;
    }

}