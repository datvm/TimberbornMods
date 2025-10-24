namespace HydroFormaProjects.Components;

public class SluiceUpstreamMarker : BaseComponent, ISelectionListener
{
    static readonly Color32 MarkerColor = new(0xFF, 0xFF, 0x00, 0xFF); // Yellow

#nullable disable
    BlockObject blockObject;
    SluiceUpstreamComponent sluiceUpstream;

    SluiceUpstreamService sluiceUpstreamService;
    MarkerDrawerFactory markerDrawerFactory;
    MeshDrawer markerDrawer;
#nullable enable

    [Inject]
    public void Inject(
        SluiceUpstreamService sluiceUpstreamService,
        MarkerDrawerFactory markerDrawerFactory
    )
    {
        this.sluiceUpstreamService = sluiceUpstreamService;
        this.markerDrawerFactory = markerDrawerFactory;
    }

    public void Awake()
    {
        blockObject = GetComponentFast<BlockObject>();
        sluiceUpstream = GetComponentFast<SluiceUpstreamComponent>();

        markerDrawer = markerDrawerFactory.CreateTileDrawer(MarkerColor);
    }

    public void Start()
    {
        enabled = false;
    }

    public void Update()
    {
        markerDrawer.DrawAtCoordinates(sluiceUpstream.ThresholdCoordinates, sluiceUpstream.Threshold + SluiceMarker.MarkerYOffset);
    }

    public void OnSelect()
    {
        if (blockObject.IsPreview 
            || !sluiceUpstreamService.IsUnlocked) { return; }

        enabled = true;
    }

    public void OnUnselect()
    {
        enabled = false;
    }
}
