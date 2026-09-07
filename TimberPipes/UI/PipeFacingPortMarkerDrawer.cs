namespace TimberPipes.UI;

[AddTemplateModule2(typeof(TransportPipeSpec))]
public class PipeFacingPortMarkerDrawer(
    MarkerDrawerFactory markerDrawerFactory,
    IBlockService blockService
) : BaseComponent, IAwakableComponent, IUpdatableComponent, IPreviewSelectionListener, IPostPlacementChangeListener
{
    static readonly Color MarkerColor = new(60 / 255f, 120 / 255f, 183 / 255f, 0.75f);

#nullable disable
    MeshDrawer meshDrawer;
    BlockObject blockObject;
    BuildingPipe buildingPipe;
    BuildingPipeSpec spec;
    Preview preview;
#nullable enable

    readonly List<Matrix4x4> markers = [];

    public void Awake()
    {
        DisableComponent();
        blockObject = GetComponent<BlockObject>();
        buildingPipe = GetComponent<BuildingPipe>();
        spec = GetComponent<BuildingPipeSpec>();
        preview = GetComponent<Preview>();
        meshDrawer = markerDrawerFactory.CreateMechanicalInputMarkerDrawer(MarkerColor);
    }

    public void Update()
    {
        if (markers.Count > 0)
        {
            meshDrawer.DrawMultiple(markers);
        }
    }

    public void OnPreviewSelect()
    {
        if (preview && preview.PreviewState.IsSingle)
        {
            EnableComponent();
            FindFacingPorts();
            return;
        }

        DisableComponent();
        markers.Clear();
    }

    public void OnPreviewUnselect()
    {
        DisableComponent();
        markers.Clear();
    }

    public void OnPostPlacementChanged()
    {
        if (Enabled)
        {
            FindFacingPorts();
        }
    }

    const int MaxDistance = 20;

    void FindFacingPorts()
    {
        markers.Clear();
        if (spec is null)
        {
            return;
        }

        foreach (var portSpec in spec.Ports)
        {
            foreach (var direction in portSpec.Directions)
            {
                var cell = blockObject.TransformCoordinates(portSpec.Coordinates);
                var outward = blockObject.TransformDirection(direction);
                var facing = outward.Across();
                var step = outward.ToOffset();

                for (var i = 1; i <= MaxDistance; i++)
                {
                    var neighborCell = cell + step * i;
                    if (!blockService.Contains(neighborCell))
                    {
                        break;
                    }

                    var blocked = false;
                    foreach (var other in blockService.GetObjectsAt(neighborCell))
                    {
                        if (other.Overridable)
                        {
                            continue;
                        }

                        blocked = true;
                        var pipe = other.GetComponent<BuildingPipe>();
                        if (pipe && pipe != buildingPipe && HasPort(pipe, neighborCell, facing))
                        {
                            markers.Add(MatrixFor(neighborCell, facing));
                        }
                    }

                    if (blocked)
                    {
                        break;
                    }
                }
            }
        }
    }

    static bool HasPort(BuildingPipe pipe, Vector3Int cell, Direction3D direction)
    {
        if (pipe.Ports is { } ports)
        {
            return ports.ContainsKey(new(cell, direction));
        }

        var otherSpec = pipe.GetComponent<BuildingPipeSpec>();
        var otherBo = pipe.GetComponent<BlockObject>();
        if (otherSpec is null || !otherBo)
        {
            return false;
        }

        foreach (var portSpec in otherSpec.Ports)
        {
            foreach (var d in portSpec.Directions)
            {
                if (otherBo.TransformCoordinates(portSpec.Coordinates) == cell
                    && otherBo.TransformDirection(d) == direction)
                {
                    return true;
                }
            }
        }

        return false;
    }

    static Matrix4x4 MatrixFor(Vector3Int coordinates, Direction3D direction)
    {
        var offset = direction.ToOffset();
        var grid = new Vector3(
            coordinates.x + offset.x * 0.5f + 0.5f,
            coordinates.y + offset.y * 0.5f + 0.5f,
            coordinates.z + offset.z * 0.5f + 0.5f);
        return Matrix4x4.TRS(CoordinateSystem.GridToWorld(grid), direction.ToRotation(), Vector3.one);
    }
}
