namespace Pin.Components;

public class PinComponent : BaseComponent, IPersistentEntity, IFinishedStateListener, IModifiableEntityBadge
{
    static readonly ComponentKey SaveKey = new(nameof(PinComponent));
    static readonly PropertyKey<Color> ColorKey = new("Color");
    static readonly PropertyKey<string> LabelKey = new("Label");
    static readonly PropertyKey<float> HeightKey = new("Height");

#nullable disable
    PinService pinService;

    Transform poleTransform;
    Renderer poleRenderer;
    LabeledEntity labeledEntity;
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

    public string Label { get; private set; } = "";
    public float Height { get; set; } = 3;
    public int EntityBadgePriority { get; } = 1;

    [Inject]
    public void Inject(PinService pinService, ILoc t)
    {
        this.pinService = pinService;
        Label = t.T("LV.Pin.NewPinText");
    }

    public void Awake()
    {
        labeledEntity = GetComponentFast<LabeledEntity>();        
    }

    public void Start()
    {
        InitializePin();
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(ColorKey))
        {
            Color = s.Get(ColorKey);
        }

        if (s.Has(LabelKey))
        {
            Label = s.Get(LabelKey);
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
        s.Set(LabelKey, Label);
        s.Set(HeightKey, Height);
    }

    public void InitializePin()
    {
        poleTransform = TransformFast.Find("#Pole");
        poleRenderer = poleTransform.GetComponent<Renderer>();
        poleRenderer.material = pinService.CreatePoleMaterial();
        UpdatePole();
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
        var coord = CoordinateSystem.GridToWorldCentered(GetComponentFast<BlockObject>().Coordinates);
        Anchor = new(coord.x, coord.y + Height, coord.z);
    }

    public void SetEntityName(string entityName)
    {
        Label = entityName;
        UpdatePin();
    }

    public string GetEntityName() => Label;
    public string GetEntitySubtitle() => "";
    public ClickableSubtitle GetEntityClickableSubtitle() => ClickableSubtitle.CreateEmpty();
    public Sprite GetEntityAvatar() => labeledEntity.Image;
}
