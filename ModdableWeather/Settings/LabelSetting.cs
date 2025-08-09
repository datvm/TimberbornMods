namespace ModdableWeather.Settings;

public class LabelSetting(string key) : NonPersistentSetting(new(null, null))
{

    public string Key { get; } = key;

    public override void Reset() { }

}

public class LabelSettingFactory(ILoc t) : IModSettingElementFactory
{
    public int Priority { get; }

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {
        if (modSetting is not LabelSetting labelSetting)
        {
            element = null;
            return false;
        }

        var label = new VisualElement().AddGameLabel(t.T(labelSetting.Key)).SetMarginBottom(5);
        element = new ModSettingElement(label, modSetting);
        return true;
    }
}
