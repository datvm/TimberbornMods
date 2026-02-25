namespace HueAndTurnX.Components;

[AddTemplateModule2(typeof(BlockObject), Contexts = BindAttributeContext.NonMenu)]
public class HueTurnComponent(
    HueTurnService hueTurnService
) : BaseComponent, IAwakableComponent, IStartableComponent
{

#nullable disable
    public TransformController TransformController { get; private set; }
    public HueTurnColorComponent Colors { get; private set; }
    public HueTurnPositionComponent Positions { get; private set; }
    public Renderer[] Renderers { get; private set; }
    BlockObject blockObject;
#nullable enable

    Vector3? originalPosition;
    Quaternion? originalRotation;
    ScaleModifier scaleModifier;

    public Material[]? ReplacedMaterials { get; set; }

    public bool IsFinished => blockObject.IsFinished;

    public Vector3Int Size => blockObject._blockObjectSpec.Size;

    bool internalModifying = false;

    public void Awake()
    {
        Renderers = GameObject.GetComponentsInChildren<Renderer>(includeInactive: true);

        TransformController = GetComponent<TransformController>();
        blockObject = GetComponent<BlockObject>();

        Colors = GetComponent<HueTurnColorComponent>();
        Positions = GetComponent<HueTurnPositionComponent>();

        Colors.OnValuesChanged += OnChanged;
        Positions.OnValuesChanged += OnChanged;

        scaleModifier = TransformController.AddScaleModifier();
    }

    public void Start()
    {
        ApplyModifications();
    }

    void ApplyModifications()
    {
        if (!IsFinished) { return; }

        hueTurnService.Apply(this);
    }

    void OnChanged()
    {
        if (internalModifying) { return; }
        ApplyModifications();
    }

    public void Clear()
    {
        internalModifying = true;
        Colors.Clear();
        Positions.Clear();
        internalModifying = false;
        ApplyModifications();
    }

    public void ResetToOriginalPositioning()
    {
        var t = Transform;
        
        if (originalPosition is null || originalRotation is null)
        {
            originalPosition = t.localPosition;
            originalRotation = t.localRotation;

        }

        t.position=  originalPosition.Value;
        t.rotation= originalRotation.Value;
    }

    public void ScaleTo(Vector3 scale)
    {
        scaleModifier.Set(scale);
    }

}
