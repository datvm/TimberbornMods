namespace Pin.Services;

public class PinService(
    IAssetLoader assets,
    IContainer container,
    CameraService cameraService,
    UISettings uiSettings,
    PinUnderlay pinUnderlay,
    EntitySelectionService entitySelectionService,
    InputService inputService,
    DrivewayModelInstantiator drivewayModelInstantiator
) : ILoadableSingleton, ILateUpdatableSingleton, IInputProcessor
{
    static readonly Shader Shader = Shader.Find("Universal Render Pipeline/Lit");

    bool isDirty;

    public Sprite PinSprite { get; private set; } = null!;
    readonly Dictionary<PinComponent, PinPanel> pins = [];

    public IReadOnlyCollection<PinComponent> Pins => pins.Keys;
    public Material PathMaterial => drivewayModelInstantiator._pathMaterial;

    Material? baseMaterial;

    public void Load()
    {
        baseMaterial = new Material(Shader);

        var texture = assets.Load<Texture2D>("Resources/Images/pin");
        PinSprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(.5f, .5f), 512f);

        cameraService.CameraPositionOrRotationChanged += (_, _) => MarkDirty();
        uiSettings.UIScaleFactorChanged += (_, _) => MarkDirty();

        inputService.AddInputProcessor(this);
    }

    public Material CreatePoleMaterial() 
        => baseMaterial ? new Material(baseMaterial) : throw new InvalidOperationException($"{nameof(PinService)} is not Loaded yet");

    public void Register(PinComponent pin)
    {
        Unregister(pin);

        var panel = CreatePanelFor(pin);
        pins.Add(pin, panel);
        pinUnderlay.Add(panel);

        UpdatePin(pin, panel);
    }

    PinPanel CreatePanelFor(PinComponent pin)
    {
        var panel = container.GetInstance<PinPanel>();
        panel.OnClicked += () => OnPanelClicked(pin);

        return panel;
    }

    void OnPanelClicked(PinComponent pin) => entitySelectionService.Select(pin);

    public void Unregister(PinComponent pin)
    {
        if (!pins.TryGetValue(pin, out var panel)) { return; }

        pinUnderlay.Remove(panel);
        pins.Remove(pin);
    }

    public void UpdatePin(PinComponent pin)
    {
        if (!pins.TryGetValue(pin, out var panel)) { return; }
        UpdatePin(pin, panel);
    }

    void UpdatePin(PinComponent pin, PinPanel panel)
    {
        panel.SetContentTo(pin);
        MarkDirty();
    }

    public void MarkDirty()
    {
        isDirty = true;
    }

    void UpdatePositions() => pinUnderlay.UpdatePositions();

    public void LateUpdateSingleton()
    {
        if (!isDirty) { return; }

        UpdatePositions();
        isDirty = false;
    }

    public bool ProcessInput()
    {
        if (inputService.IsKeyDown("TogglePin"))
        {
            pinUnderlay.ToggleVisibility();
        }

        return false;
    }

}
