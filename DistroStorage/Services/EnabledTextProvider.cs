namespace DistroStorage.Services;

[BindSingleton]
public class EnabledTextProvider(ILoc t) : ILoadableSingleton
{

    public string EnabledText { get; private set; } = "";
    public string DisabledText { get; private set; } = "";

    public void Load()
    {
        EnabledText = t.T("LV.DS.Enabled");
        DisabledText = t.T("LV.DS.Disabled");
    }

    public string Get(bool enabled) => enabled ? EnabledText : DisabledText;

}
