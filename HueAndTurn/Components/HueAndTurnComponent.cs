namespace HueAndTurn.Components;

public class HueAndTurnComponent : BaseComponent, IPersistentEntity, IDeletableEntity
{
    static readonly ComponentKey SaveKey = new("HueAndTurn");

    public HueAndTurnProperties Properties { get; private set; } = new();
    public bool RotationPivotSupported => positionModifier is null;

    Vector3? originalPosition;
    Quaternion? originalRotation;

    Transform _transform = null!;
    BlockObject blockObject = null!;
    Renderer renderer = null!;

    LabeledEntity? labeledEntity;
    PositionModifier? positionModifier;
    ScaleModifier? scaleModifier;

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
        if (positionModifier is null)
        {
            _transform.Translate(rotationPivot);
        }
        _transform.Rotate(Vector3.up, rotation, Space.Self);

        if (Properties.RotationXZ.HasValue)
        {
            var rotX = Properties.RotationXZ.Value.x;
            var rotZ = Properties.RotationXZ.Value.y;

            if (rotX != 0)
            {
                _transform.Rotate(Vector3.right, rotX, Space.Self);
            }

            if (rotZ != 0)
            {
                _transform.Rotate(Vector3.forward, rotZ, Space.Self);
            }
        }

        if (positionModifier is null)
        {
            _transform.Translate(-rotationPivot);
        }

        // Translate
        if (positionModifier is null)
        {
            _transform.Translate(translation, Space.World);
        }
        else
        {
            positionModifier.Set(translation);
        }

        // Scale
        if (scaleModifier is not null)
        {
            var scale = Vector3.one + (Vector3)(Properties.Scale ?? Vector3Int.zero) / 100f;
            scale = CoordinateSystem.GridToWorld(scale);

            scaleModifier.Set(scale);
        }
    }

    void ResetPositioning()
    {
        if (positionModifier is null)
        {
            _transform.position = originalPosition!.Value;
        }
        else
        {
            positionModifier.Reset();
        }

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
        var transformController = GetComponentFast<TransformController>();

        if (GetComponentFast<NaturalResourceModel>())
        {
            positionModifier = transformController?.AddPositionModifier();
        }
        scaleModifier = transformController?.AddScaleModifier();

        if (Properties.Color is not null)
        {
            ApplyColor();
        }

        ApplyRepositioning();
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        
        Properties = HueAndTurnProperties.Load(s);
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
