namespace QuickBar.UI;

public class QuickBarController(
    QuickBarElement display,
    ISingletonLoader loader
) : ILoadableSingleton, ISaveableSingleton
{
    static readonly PropertyKey<bool> VerticalBarKey = new("VerticalBar");

    public bool IsVertical { get; private set; }

    public void Load()
    {
        LoadSavedData();
        display.SwitchLocation(IsVertical);

        display.OnChangeLocationRequested += SwitchLocation;
    }

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(QuickBarModUtils.SaveKey, out var s)) { return; }

        if (s.Has(VerticalBarKey) && s.Get(VerticalBarKey))
        {
            IsVertical = true;
        }
    }

    public void SwitchLocation()
    {
        IsVertical = !IsVertical;
        display.SwitchLocation(IsVertical);
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(QuickBarModUtils.SaveKey);
        if (IsVertical) { s.Set(VerticalBarKey, true); }
    }
}
