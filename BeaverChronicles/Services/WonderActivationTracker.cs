namespace BeaverChronicles.Services;

[BindSingleton]
public class WonderActivationTracker(DefaultEntityTracker<Wonder> wonders) : ILoadableSingleton
{
    public event EventHandler<Wonder>? WonderActivated;

    public void Load()
    {
        wonders.OnEntityRegistered += OnEntityRegistered;
    }

    void OnEntityRegistered(Wonder w)
    {
        w.WonderActivated += OnWonderActivated;
    }

    void OnWonderActivated(object sender, EventArgs e) 
        => WonderActivated?.Invoke(sender, (Wonder)sender);

}
