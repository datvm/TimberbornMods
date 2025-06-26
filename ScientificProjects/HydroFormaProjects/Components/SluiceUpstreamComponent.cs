namespace HydroFormaProjects.Components;

public class SluiceUpstreamComponent : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new("SluiceUpstream");
    static readonly PropertyKey<bool> AutoOpenKey = new("AutoOpen");
    static readonly PropertyKey<float> ThresholdKey = new("Threshold");

    public bool AutoOpen { get; set; }
    public float Threshold { get; set; }
    public float MaxThreshold { get; private set; }

#nullable disable
    MapSize mapSize;
#nullable enable

    [Inject]
    public void Inject(MapSize mapSize)
    {
        this.mapSize = mapSize;
    }

    public void Start()
    {
        var bo = GetComponentFast<BlockObject>();
        MaxThreshold = mapSize.TotalSize.z - bo.Coordinates.z;
    }

    public void Load(IEntityLoader loader)
    {
        if (!loader.TryGetComponent(SaveKey, out var s)) { return; }
        if (s.Has(AutoOpenKey)) { AutoOpen = s.Get(AutoOpenKey); }
        if (s.Has(ThresholdKey)) { Threshold = s.Get(ThresholdKey); }
    }

    public void Save(IEntitySaver saver)
    {
        var s = saver.GetComponent(SaveKey);
        
        if (AutoOpen)
        {
            s.Set(AutoOpenKey, AutoOpen);
        }

        s.Set(ThresholdKey, Threshold);
    }
}
