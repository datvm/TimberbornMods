namespace BeaverChronicles.Services;

[BindSingleton]
public class WonderActivationTracker(
    DefaultEntityTracker<Wonder> wonders,
    FlagHelper flagHelper
) : ILoadableSingleton
{
    public event EventHandler<Wonder>? WonderActivated;
    public bool HasEverActivatedWonder => flagHelper.IsWonderActivated;

    public void Load()
    {
        wonders.OnEntityRegistered += OnEntityRegistered;
    }

    void OnEntityRegistered(Wonder w)
    {
        w.WonderActivated += OnWonderActivated;
    }

    void OnWonderActivated(object sender, EventArgs e)
    {
        flagHelper.MarkWonderActivated();
        WonderActivated?.Invoke(sender, (Wonder)sender);
    }
}
