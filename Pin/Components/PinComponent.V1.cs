namespace Pin.Components;

public class PinComponent(PinService pinService) : BaseComponent, IDuplicable<PinComponent>, IPersistentEntity, IFinishedStateListener, IAwakableComponent, IStartableComponent
{
    static readonly ComponentKey SaveKey = new(nameof(PinComponent));
    static readonly PropertyKey<Color> ColorKey = new("Color");
    static readonly PropertyKey<float> HeightKey = new("Height");

#nullable disable
    Transform finished;
    Transform poleTransform;
    Renderer poleRenderer;
    NamedEntity namedEntity;
#nullable enable

    public Vector3 Anchor { get; private set; }

    public Color Color
    {
        get;
        set
        {
            field = value;
            ColorWithoutAlpha = value with { a = 1, };
            ColorAlpha = value.a;
        }
    } = Color.white;
    public Color ColorWithoutAlpha { get; private set; } = Color.white;
    public float ColorAlpha { get; private set; } = 1;

    public string Label => namedEntity.EntityName;
    public float Height { get; set; } = 3;
    public int EntityBadgePriority { get; } = 1;

    public void Awake()
    {
        finished = Transform.Find("#Finished");

        namedEntity = GetComponent<NamedEntity>();
        InitializePole();
    }

    public void Start()
    {
        InitializePin();
        namedEntity.EntityNameChanged += (_, _) => UpdatePin();
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(ColorKey))
        {
            Color = s.Get(ColorKey);
        }

        if (s.Has(HeightKey))
        {
            Height = s.Get(HeightKey);
        }
    }

    public void OnEnterFinishedState()
    {
        UpdateAnchor();
        pinService.Register(this);
    }

    public void OnExitFinishedState() { pinService.Unregister(this); }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(ColorKey, Color);
        s.Set(HeightKey, Height);
    }

    public void InitializePin()
    {
        var pathRenderers = finished.GetComponentsInChildren<Renderer>();
        var material = pinService.PathMaterial;
        foreach (var r in pathRenderers)
        {
            r.sharedMaterial = material;
        }

        poleRenderer = poleTransform.GetComponent<Renderer>();
        poleRenderer.material = pinService.CreatePoleMaterial();
        UpdatePole();
    }

    void InitializePole()
    {
        poleTransform = CreatePole(Transform);
    }

    static Transform CreatePole(Transform parent)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        var t = obj.transform;
        t.parent = parent;

        t.localPosition = new(.5f, 1.5f, .5f);
        t.localScale = new(.1f, 1.5f, .1f);

        return t;
    }

    void UpdatePole()
    {
        poleTransform.localScale = poleTransform.localScale with { y = Height / 2f };
        poleTransform.localPosition = poleTransform.localPosition with { y = Height / 2f };

        var m = poleRenderer.material;
        m.color = Color;
    }

    public void UpdatePin()
    {
        UpdatePole();

        UpdateAnchor();
        pinService.UpdatePin(this);
    }

    void UpdateAnchor()
    {
        var coord = CoordinateSystem.GridToWorldCentered(GetComponent<BlockObject>().Coordinates);
        Anchor = new(coord.x, coord.y + Height, coord.z);
    }

    public void SetEntityName(string entityName) => namedEntity.SetEntityName(entityName);

    public void DuplicateFrom(PinComponent source)
    {
        Color = source.Color;
        Height = source.Height;
        SetEntityName(source.Label);
        // No need to call UpdatePin here, as it will be called by the named entity change event.
        // It will crash if called too early anyway (for build + duplicate).
    }
}
