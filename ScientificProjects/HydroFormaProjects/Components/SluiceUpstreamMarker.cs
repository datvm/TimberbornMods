namespace HydroFormaProjects.Components;

public class SluiceUpstreamMarker(
    SluiceUpstreamService sluiceUpstreamService,
    MarkerDrawerFactory markerDrawerFactory
) : BaseComponent, ISelectionListener, IAwakableComponent, IStartableComponent, IUpdatableComponent
{
    static readonly Color32 MarkerColor = new(0xFF, 0xFF, 0x00, 0xFF); // Yellow

#nullable disable
    BlockObject blockObject;
    SluiceUpstreamComponent sluiceUpstream;
    MeshDrawer markerDrawer;
#nullable enable

    public void Awake()
    {
        blockObject = GetComponent<BlockObject>();
        sluiceUpstream = GetComponent<SluiceUpstreamComponent>();

        markerDrawer = markerDrawerFactory.CreateTileDrawer(MarkerColor);
    }

    public void Start()
    {
        DisableComponent();
    }

    public void Update()
    {
        markerDrawer.DrawAtCoordinates(sluiceUpstream.ThresholdCoordinates, sluiceUpstream.Threshold + SluiceMarker.MarkerYOffset);
    }

    public void OnSelect()
    {
        if (blockObject.IsPreview 
            || !sluiceUpstreamService.IsUnlocked) { return; }

        EnableComponent();
    }

    public void OnUnselect()
    {
        DisableComponent();
    }
}
