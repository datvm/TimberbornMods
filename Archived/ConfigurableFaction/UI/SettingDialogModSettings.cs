namespace ConfigurableFaction.UI;

public class SettingDialogModSetting(FactionOptionsProvider options) : NonPersistentSetting(new("", ""))
{

    public override void Reset()
    {
        options.Reset();
        base.Reset();
    }

}

public class SettingDialogModSettingFactory(IContainer container) : IModSettingElementFactory
{
    public int Priority { get; }

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {
        if (modSetting is not SettingDialogModSetting)
        {
            element = null;
            return false;
        }

        element = new ModSettingElement(container.GetInstance<SettingDialog>().Init(), modSetting);
        return true;
    }
}