namespace SluiceIsBack.UI;

#nullable disable

public class SluiceMarker(MarkerDrawerFactory markerDrawerFactory) : BaseComponent, IAwakableComponent, IUpdatableComponent, ISelectionListener
{
    static readonly Color32 MarkerColor = Color.blue;

    static readonly float MarkerYOffset = 0.02f;
    Sluice _sluice;

    SluiceState _sluiceState;

    MeshDrawer _markerDrawer;

    BlockObject _blockObject;

    public void Awake()
    {
        _sluice = GetComponent<Sluice>();
        _sluiceState = GetComponent<SluiceState>();
        _blockObject = GetComponent<BlockObject>();
        _markerDrawer = markerDrawerFactory.CreateTileDrawer(MarkerColor);
        DisableComponent();
    }

    public void Update()
    {
        Vector3Int targetCoordinates = _sluice.TargetCoordinates;
        Vector3Int coordinates = new Vector3Int(targetCoordinates.x, targetCoordinates.y, _sluice.MaxHeight);
        _markerDrawer.DrawAtCoordinates(coordinates, _sluiceState.OutflowLimit + MarkerYOffset);
    }

    public void OnSelect()
    {
        if (!_blockObject.IsPreview)
        {
            EnableComponent();
        }
    }

    public void OnUnselect()
    {
        DisableComponent();
    }
}
