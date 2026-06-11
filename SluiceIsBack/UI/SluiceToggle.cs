namespace SluiceIsBack.UI;

#nullable disable

public class SluiceToggle
{
    // Previous game values, kept for reference only.
    public static readonly string AutoClass = "sluice-toggle__icon--auto";
    public static readonly string OpenClass = "sluice-toggle__icon--open";
    public static readonly string CloseClass = "sluice-toggle__icon--close";

    // No more css classes
    const string AutoIcon = "ico-automated";
    const string OpenIcon = "sluice-open";
    const string CloseIcon = "sluice-close";

    static readonly string AutoLocKey = "Building.Sluice.Mode.Auto";

    static readonly string OpenLocKey = "Building.Sluice.Mode.Open";

    static readonly string ClosedLocKey = "Building.Sluice.Mode.Closed";

    readonly SliderToggleFactory _sliderToggleFactory;

    readonly ILoc _loc;
    readonly IAssetLoader assets;
    SluiceState _sluiceState;

    SliderToggle _sliderToggle;

    public SluiceToggle(SliderToggleFactory sliderToggleFactory, ILoc loc, IAssetLoader assets)
    {
        _sliderToggleFactory = sliderToggleFactory;
        _loc = loc;
        this.assets = assets;
    }

    public void Initialize(VisualElement parent)
    {
        var autoIcon = GetIcon(AutoIcon);
        var openIcon = GetIcon(OpenIcon);
        var closeIcon = GetIcon(CloseIcon);

        Debug.Log(autoIcon);

        SliderToggleItem auto = SliderToggleItem.Create(() => _loc.T(AutoLocKey), autoIcon, () => _sluiceState.SetAuto(), () => _sluiceState.AutoMode);
        SliderToggleItem open = SliderToggleItem.Create(() => _loc.T(OpenLocKey), openIcon, () => _sluiceState.Open(), () => !_sluiceState.AutoMode && _sluiceState.IsOpen);
        SliderToggleItem close = SliderToggleItem.Create(() => _loc.T(ClosedLocKey), closeIcon, () => _sluiceState.Close(), () => !_sluiceState.AutoMode && !_sluiceState.IsOpen);

        _sliderToggle = _sliderToggleFactory.Create(parent, auto, open, close);

        Sprite GetIcon(string name) => assets.Load<Sprite>("Resources/UI/Images/Game/Sluice/" + name);
    }

    public void Show(SluiceState sluiceState)
    {
        _sluiceState = sluiceState;
    }

    public void Update()
    {
        if ((bool)_sluiceState)
        {
            _sliderToggle.Update();
        }
    }

    public void Clear()
    {
        _sluiceState = null;
    }
}
