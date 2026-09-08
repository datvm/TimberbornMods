namespace HueAndTurn.Components;

public class HueAndTurnComponent : BaseComponent, IPersistentEntity, IHueAndTurnComponent
{
    public const string EnvironmentURPName = "EnvironmentURP";
    public const string WaterURPName = "WaterURP";

    static readonly ComponentKey SaveKey = new("HueAndTurn");

    HueAndTurnProperties properties = new();
    public ReadOnlyHueAndTurnProperties Properties => new(properties);

    public bool RotationPivotSupported => positionModifier is null;

    Vector3? originalPosition;
    Quaternion? originalRotation;
    bool hasSetTransparency;

#nullable disable
    Transform _transform;
    BlockObject blockObject;
    TransparencyShaderService transparencyShaderService;
#nullable enable

    LabeledEntity? labeledEntity;
    PositionModifier? positionModifier;
    ScaleModifier? scaleModifier;

    public PrefabSpec? PrefabSpec { get; private set; }
    public string PrefabName => PrefabSpec?.PrefabName ?? name;
    public string DisplayName => labeledEntity?.DisplayName ?? PrefabName;

    public bool HasFluid => GatherFluidMaterial().Any();
    public bool CanHaveTransparency { get; private set; }

    ColorHighlighter highlighter = null!;

    [Inject]
    public void Inject(ColorHighlighter highlighter, TransparencyShaderService transparencyShaderService)
    {
        this.highlighter = highlighter;
        this.transparencyShaderService = transparencyShaderService;
    }

    void ApplyColor()
    {
        if (properties.Color is null)
        {
            ClearColor();
            return;
        }

        highlighter.SetColor(this, properties.Color.Value);
    }

    void ApplyTransparency()
    {
        var transparency = properties.Transparency ?? 100;
        if (!hasSetTransparency && transparency >= 100) { return; }

        var alpha = Mathf.Clamp(transparency / 100f, 0f, 1f);
        hasSetTransparency = true;

        var materials = transparencyShaderService.ReplaceMaterials(GameObjectFast);
        foreach (var m in materials)
        {
            m.SetFloat("_Alpha", alpha);
        }
    }

    void ApplyFluidColor()
    {
        var color = properties.FluidColor;

        foreach (var m in GatherFluidMaterial())
        {
            var target = color ?? MaterialColorer.UnhighlightedColor;
            m.SetColor("_Color", target);
        }
    }

    void ClearColor()
    {
        highlighter.ResetColor(this);
    }

    void ApplyRepositioning()
    {
        originalPosition ??= _transform.position;
        originalRotation ??= _transform.rotation;

        var rotation = properties.Rotation ?? 0; // Degree, from -180 to 180
        var pivot = properties.RotationPivot ?? Vector2Int.zero; // Percentage value, from -50% to 50% of the size
        var translation = CoordinateSystem.GridToWorld(properties.Translation ?? Vector3.zero); // Same as above

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

        if (properties.RotationXZ.HasValue)
        {
            var rotX = properties.RotationXZ.Value.x;
            var rotZ = properties.RotationXZ.Value.y;

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
            var scale = Vector3.one + (Vector3)(properties.Scale ?? Vector3Int.zero) / 100f;
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
        ApplyTransparency();
        ApplyRepositioning();
        ApplyFluidColor();
    }

    public void SetColor(Color? color)
    {
        properties.Color = color;
        ApplyColor();
    }

    public void SetFluidColor(Color? color)
    {
        properties.FluidColor = color;
        ApplyFluidColor();
    }

    public void SetTransparency(int? transparency)
    {
        properties.Transparency = transparency;
        ApplyTransparency();
    }

    public void SetRotation(int? rotation)
    {
        properties.Rotation = rotation;
        ApplyRepositioning();
    }

    public void SetRotationWithPivot(int? rotation, Vector2Int? rotationPivot)
    {
        properties.Rotation = rotation;
        properties.RotationPivot = rotationPivot;
        ApplyRepositioning();
    }

    public void SetRotationXZ(Vector2Int? rotationXZ)
    {
        properties.RotationXZ = rotationXZ;
        ApplyRepositioning();
    }

    public void SetRotationPivot(Vector2Int? rotationPivot)
    {
        properties.RotationPivot = rotationPivot;
        ApplyRepositioning();
    }

    public void SetTranslation(Vector3Int? translation)
    {
        properties.Translation = translation;
        ApplyRepositioning();
    }

    public void SetScale(Vector3Int? scale)
    {
        properties.Scale = scale;
        ApplyRepositioning();
    }

    public void ApplyProperties(ReadOnlyHueAndTurnProperties properties)
    {
        this.properties = properties.ToProperties();
        ApplyEverything();
    }

    public void Reset()
    {
        properties = new HueAndTurnProperties();
        ApplyEverything();
    }

    public void Awake()
    {
        _transform = TransformFast;
        blockObject = GetComponentFast<BlockObject>();
        PrefabSpec = GetComponentFast<PrefabSpec>();
        labeledEntity = GetComponentFast<LabeledEntity>();

        CanHaveTransparency = !GetComponentFast<BuildingTerrainCutoutSpec>();
    }

    public void Start()
    {
        GatherFluidMaterial();

        var transformController = GetComponentFast<TransformController>();

        if (GetComponentFast<NaturalResourceModel>())
        {
            positionModifier = transformController?.AddPositionModifier();
        }
        scaleModifier = transformController?.AddScaleModifier();

        if (properties.Color is not null)
        {
            ApplyColor();
        }

        ApplyTransparency();
        ApplyRepositioning();
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        properties = HueAndTurnProperties.Load(s);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (properties == default) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        properties.Save(s);
    }

    IEnumerable<Material> GatherFluidMaterial() => GatherMaterialOf(WaterURPName);

    IEnumerable<Material> GatherMaterialOf(string shaderName)
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            foreach (var m in r.materials)
            {
                if (m.shader.name.Contains(shaderName))
                {
                    yield return m;
                }
            }
        }
    }

}

public record FluidMaterialInfo(Material Material, Color OriginalColor);