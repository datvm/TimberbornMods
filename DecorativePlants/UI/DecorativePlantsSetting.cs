namespace DecorativePlants.UI;

public class DecorativePlantsSetting() : NonPersistentSetting(new(null, null));

[MultiBind(typeof(IModSettingElementFactory), Contexts = BindAttributeContext.MainMenu)]
public class DecorativePlantsSettingFactory(IContainer container) : IModSettingElementFactory
{
    public int Priority { get; }

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {
        if (modSetting is not DecorativePlantsSetting setting)
        {
            element = null;
            return false;
        }

        var panel = container.GetInstance<DecorativePlantsSettingPanel>();
        element = new ModSettingElement(panel, modSetting);
        return true;
    }
}
