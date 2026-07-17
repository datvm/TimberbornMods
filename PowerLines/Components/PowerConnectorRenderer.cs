namespace PowerLines.Components;

/// <summary>
/// Draws small spheres at each power-line connector point while the building is selected
/// (or shown as a placement preview) — same idea as mechanical transput markers.
/// </summary>
[AddTemplateModule2(typeof(PowerLineComponent))]
public class PowerConnectorRenderer(
    PowerLineConnectorRenderer connectorRenderer
) : BaseComponent, IAwakableComponent, IUpdatableComponent, ISelectionListener, IPreviewSelectionListener, IPostPlacementChangeListener
{
    PowerLineComponent powerLine = null!;
    BlockObject blockObject = null!;
    readonly List<Matrix4x4> markers = [];

    public void Awake()
    {
        powerLine = GetComponent<PowerLineComponent>();
        blockObject = GetComponent<BlockObject>();
        DisableComponent();
    }

    public void Update()
    {
        if (markers.Count == 0) { return; }
        connectorRenderer.Drawer.DrawMultiple(markers);
    }

    public void OnSelect()
    {
        EnableAndRebuild();
    }

    public void OnUnselect()
    {
        Disable();
    }

    public void OnPreviewSelect()
    {
        EnableAndRebuild();
    }

    public void OnPreviewUnselect()
    {
        Disable();
    }

    public void OnPostPlacementChanged()
    {
        if (Enabled)
        {
            RebuildMarkers();
        }
    }

    void EnableAndRebuild()
    {
        RebuildMarkers();
        if (markers.Count > 0)
        {
            EnableComponent();
        }
        else
        {
            DisableComponent();
        }
    }

    void Disable()
    {
        DisableComponent();
        markers.Clear();
    }

    void RebuildMarkers()
    {
        markers.Clear();

        var locations = powerLine.ConnectionLocations;
        if (locations.IsDefaultOrEmpty)
        {
            // Previews may not have run InitializeEntity yet — derive from spec if possible.
            locations = ResolvePreviewLocations();
        }

        foreach (var gridPos in locations)
        {
            markers.Add(connectorRenderer.MatrixAtGridPosition(gridPos));
        }
    }

    ImmutableArray<Vector3> ResolvePreviewLocations()
    {
        var spec = GetComponent<PowerLineSpec>();
        var local = spec?.ConnectionLocations ?? [];
        if (local.Length > 0)
        {
            return [.. local.Select(blockObject.TransformCoordinates)];
        }

        // Fallback: top center of occupied blocks (same as PowerLineComponent)
        if (!blockObject || blockObject.PositionedBlocks is null)
        {
            return [];
        }

        var occupied = blockObject.PositionedBlocks.GetOccupiedBlocks();
        if (!occupied.Any()) { return []; }

        var occupiedTop = occupied.Max(b => b.Coordinates.z);
        var center = GetComponent<BlockObjectCenter>();
        if (!center) { return []; }

        return [center.GridCenter with { z = occupiedTop + .98f }];
    }
}
