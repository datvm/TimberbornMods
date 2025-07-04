namespace BeaverJukebox.UI;

public class JukeboxModSetting() : NonPersistentSetting(ModSettingDescriptor.Create(""))
{

    public event Action? OnResetRequested;

    public override void Reset()
    {
        OnResetRequested?.Invoke();
    }

}

public class JukeboxModSettingFactory(IContainer container) : IModSettingElementFactory
{
    public int Priority { get; }

    public bool TryCreateElement(ModSetting modSetting, out IModSettingElement? element)
    {
        if (modSetting is JukeboxModSetting)
        {
            var panel = container.GetInstance<JukeboxModSettingPanel>();
            element = new ModSettingElement(panel, modSetting);
            return true;
        }

        element = null;
        return false;
    }

}