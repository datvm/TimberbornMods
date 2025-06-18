namespace QuickBar.UI;

public class QuickBarController(
    QuickBarElement quickBarEl,
    ISingletonLoader loader
) : ILoadableSingleton, ISaveableSingleton, IPostLoadableSingleton
{
    static readonly PropertyKey<bool> VerticalBarKey = new("VerticalBar");

    public bool IsVertical { get; private set; }

    public void Load()
    {
        LoadSavedData();
        quickBarEl.OnChangeLocationRequested += SwitchLocation;
    }

    public void PostLoad()
    {
        quickBarEl.SwitchLocation(IsVertical);
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
        quickBarEl.SwitchLocation(IsVertical);
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(QuickBarModUtils.SaveKey);
        if (IsVertical) { s.Set(VerticalBarKey, true); }
    }

}
