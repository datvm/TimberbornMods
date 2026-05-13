
namespace BeaverChroniclesCampfire.Helpers;

[BindSingleton]
public class FloodingHelper(
    DefaultEntityTracker<FloodableObject> tracker
)
{

    public bool Listening { get; private set; }
    public event EventHandler? OnBuildingFlooded;

    public void StartListen()
    {
        if (Listening) { return; }
        Listening = true;

        tracker.OnEntityRegistered += OnEntityRegistered;
        tracker.OnEntityUnregistered += OnEntityUnregistered;
        foreach (var e in tracker.Entities)
        {
            OnEntityRegistered(e);
        }
    }

    void OnEntityRegistered(FloodableObject obj)
    {
        obj.Flooded += OnFlooded;
    }

    void OnEntityUnregistered(FloodableObject obj)
    {
        obj.Flooded -= OnFlooded;
    }


    void OnFlooded(object sender, EventArgs e)
    {
        if (!Listening) { return; }
        
        OnBuildingFlooded?.Invoke(this, EventArgs.Empty);
    }

    public void StopListening()
    {
        if (!Listening) { return; }
        Listening = false;

        tracker.OnEntityRegistered -= OnEntityRegistered;
        tracker.OnEntityUnregistered -= OnEntityUnregistered;
        foreach (var e in tracker.Entities)
        {
            OnEntityUnregistered(e);
        }
    }

}
