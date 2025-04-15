global using Timberborn.BlockSystem;
global using Timberborn.Coordinates;
global using Timberborn.EntitySystem;
global using Timberborn.SelectionSystem;

namespace HueAndTurn.Components;

public class HueAndTurnComponent : BaseComponent, IPersistentEntity, IDeletableEntity
{
    static readonly ComponentKey LegacySaveKey = new("BuildingColor");
    static readonly ComponentKey SaveKey = new("HueAndTurn");

    public HueAndTurnProperties Properties { get; private set; } = new();

    Vector3? originalPosition;
    Quaternion? originalRotation;

    Transform _transform = null!;
    BlockObject blockObject = null!;
    Renderer renderer = null!;

    LabeledEntity? labeledEntity;
    public PrefabSpec? PrefabSpec { get; private set; }
    public string PrefabName => PrefabSpec?.PrefabName ?? name;
    public string DisplayName => labeledEntity?.DisplayName ?? PrefabName;

    ColorHighlighter highlighter = null!;

    [Inject]
    public void Inject(ColorHighlighter highlighter)
    {
        this.highlighter = highlighter;
    }

    public void ApplyColor()
    {
        if (Properties.Color is null)
        {
            ClearColor();
            return;
        }

        highlighter.SetColor(this, Properties.Color.Value);
    }

    void ClearColor()
    {
        highlighter.ResetColor(this);
    }

    public void ApplyRepositioning()
    {
        originalPosition ??= _transform.position;
        originalRotation ??= _transform.rotation;

        var rotation = Properties.Rotation ?? 0; // Degree, from -180 to 180
        var pivot = Properties.RotationPivot ?? Vector2Int.zero; // Percentage value, from -50% to 50% of the size
        var translation = CoordinateSystem.GridToWorld(Properties.Translation ?? Vector3.zero); // Same as above

        ResetPositioning();

        var size = CoordinateSystem.GridToWorld(blockObject.BlocksSpec.Size);
        var rotationPivot = new Vector3(
            size.x * ((50f + pivot.x) / 100f),
            0,
            size.z * ((50f + pivot.y) / 100f)
        );
        translation.Scale(size / 100f);

        // Rotate
        _transform.Translate(rotationPivot);
        _transform.Rotate(Vector3.up, rotation, Space.Self);
        _transform.Translate(-rotationPivot);

        // Translate
        _transform.Translate(translation, Space.World);
    }

    void ResetPositioning()
    {
        _transform.position = originalPosition!.Value;
        _transform.rotation = originalRotation!.Value;
    }

    void ApplyEverything()
    {
        ApplyColor();
        ApplyRepositioning();
    }

    public void ApplyProperties(HueAndTurnProperties properties)
    {
        Properties = properties with { };
        ApplyEverything();
    }

    public void Reset()
    {
        if (Properties.IsDefault) { return; }

        Properties = new HueAndTurnProperties();
        ApplyEverything();
    }

    public void Awake()
    {
        renderer = GetComponentInChildren<Renderer>(true);
        _transform = TransformFast;
        blockObject = GetComponentFast<BlockObject>();
        PrefabSpec = GetComponentFast<PrefabSpec>();
        labeledEntity = GetComponentFast<LabeledEntity>();
    }

    public void Start()
    {
        if (Properties.Color is not null)
        {
            ApplyColor();
        }

        if (Properties.Rotation is not null || Properties.Translation is not null)
        {
            ApplyRepositioning();
        }
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        entityLoader.TryGetComponent(LegacySaveKey, out var legacy);

        Properties = HueAndTurnProperties.Load(s, legacy);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (Properties == default) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        Properties.Save(s);
    }

    public void DeleteEntity()
    {
        var material = renderer?.material;
        if (material is not null)
        {
            Destroy(material);
        }
    }
}
