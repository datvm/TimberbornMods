namespace BlockObjectDebugger.Components;

public class BlockObjectDebuggerComponentSpec : BaseComponent
{
    [SerializeField]
    public BlockOccupations occupations;

    public BlockOccupations Occupation => occupations;
}

public class BlockObjectDebuggerComponent : BaseComponent
{
    static readonly Vector3 DefaultPosition = new(0.5f, 0.5f, 0.5f);
    const float Thickness = .1f;
    const float HalfThickness = Thickness / 2f;

#nullable disable
    BlockObject blockObject;
    BlockObjectDebuggerMaterialService service;
#nullable enable

    ImmutableArray<GameObject> debugObjs = [];
    public bool DrawingDebug => debugObjs.Length > 0;

    [Inject]
    public void Inject(BlockObjectDebuggerMaterialService service)
    {
        this.service = service;
    }

    public void Awake()
    {
        blockObject = GetComponentFast<BlockObject>();
    }

    public void Start()
    {
        if (GetComponentFast<BlockObjectDebuggerComponentSpec>())
        {
            ToggleDebug(true);
        }
    }

    public void ToggleDebug(bool enabled)
    {
        RemoveDebugObjects();

        if (enabled)
        {
            DrawDebugObjects();
        }
    }

    public string Info
    {
        get
        {
            var str = new StringBuilder();

            var blocks = blockObject.Blocks;

            str.AppendLine($"Base Coordinate: {blockObject.Coordinates.ToCoordsString()}");
            str.AppendLine($"Size: {blocks.Size.ToCoordsString()}");
            str.AppendLine($"Blocks:");

            foreach (var block in blocks.GetAllBlocks())
            {
                str.AppendLine($"› {block.Coordinates.ToCoordsString()}:");
                str.AppendLine($"  - Occupation: {block.Occupation}");
                str.AppendLine($"  - MatterBelow: {block.MatterBelow}");
            }

            return str.ToString();
        }
    }

    public IEnumerable<Vector3Int> PositionedCoordinates => blockObject.PositionedBlocks.GetAllCoordinates();

    void DrawDebugObjects()
    {
        List<GameObject> gos = [];

        foreach (var block in blockObject.Blocks.GetAllBlocks())
        {
            var occupation = block.Occupation;

            foreach (var o in ModUtils.AllOccupations)
            {
                if (occupation == BlockOccupations.None) { continue; }

                if (occupation.HasFlag(o))
                {
                    var objs = DrawOccupation(o, block);

                    foreach (var obj in objs)
                    {
                        gos.Add(obj);
                    }
                }
            }
        }

        debugObjs = [.. gos];
    }

    void RemoveDebugObjects()
    {
        if (debugObjs.Length == 0) { return; }

        foreach (var obj in debugObjs)
        {
            Destroy(obj);
        }
        debugObjs = [];
    }

    IEnumerable<GameObject> DrawOccupation(BlockOccupations occupation, Block block)
    {
        GameObject go;
        var parentTransform = TransformFast;
        if (occupation == BlockOccupations.Corners)
        {
            var (positions, scale) = GetCornerTransform();

            foreach (var pos in positions)
            {
                CreateGameObject(pos, scale);
                yield return go;
            }
        }
        else
        {
            var (pos, scale) = GetTransform(occupation);
            CreateGameObject(pos, scale);
            yield return go;
        }

        void CreateGameObject(Vector3 pos, Vector3 scale)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //go.RemoveComponent<Collider>();

            ConfigureRenderer();
            ConfigureTransform(pos, scale);
        }

        void ConfigureTransform(Vector3 pos, Vector3 scale)
        {
            var t = go.transform;
            t.parent = parentTransform;

            t.localPosition = CoordinateSystem.GridToWorld(block.Coordinates) + pos;
            t.localScale = scale;
        }

        void ConfigureRenderer()
        {
            var renderer = go.GetComponent<Renderer>();
            renderer.material = service.GetMaterial(occupation);
        }
    }

    public static (Vector3 Position, Vector3 Scale) GetTransform(BlockOccupations occupations) => occupations switch
    {
        BlockOccupations.None => (DefaultPosition, Vector3.zero),
        BlockOccupations.Floor => (DefaultPosition with { y = HalfThickness }, new(1f, Thickness, 1f)),
        BlockOccupations.Bottom => (DefaultPosition with { y = HalfThickness + Thickness }, new(1f, Thickness, 1f)),
        BlockOccupations.Top => (DefaultPosition with { y = 1 - HalfThickness }, new(1f, Thickness, 1f)),
        BlockOccupations.Path => (DefaultPosition with { y = HalfThickness + Thickness * 2 }, new(1f, Thickness, 1f)),
        BlockOccupations.Middle => (DefaultPosition, new(.8f, .8f, .8f)),
        BlockOccupations.Corners => throw new InvalidOperationException($"Call {nameof(GetCornerTransform)} instead."),
        _ => throw new ArgumentOutOfRangeException(nameof(occupations), occupations, null),
    };

    public static (Vector3[] Positions, Vector3 Scale) GetCornerTransform() => (
        [
            DefaultPosition with { x = HalfThickness, z = HalfThickness, },
            DefaultPosition with { x = 1 - HalfThickness, z = HalfThickness, },
            DefaultPosition with { x = HalfThickness, z = 1 - HalfThickness, },
            DefaultPosition with { x = 1 - HalfThickness, z = 1 - HalfThickness, },
        ],
        new(Thickness, 1, Thickness)
    );

}
