namespace Pin.UI;

public class PinPanel : VisualElement
{
    public PinComponent Pin { get; private set; } = null!;
    public Vector3 Anchor { get; private set; }

    readonly Image icon;
    readonly Label lblText;

    public event Action OnClicked = null!;

    public PinPanel(PinService pinService)
    {
        style.position = Position.Absolute;

        var frame = this.AddChild<NineSliceVisualElement>(classes: [UiCssClasses.FragmentBgPrefix + UiCssClasses.Frame]);
        var content = frame.AddChild<NineSliceButton>(classes: [UiCssClasses.FragmentBgPrefix + UiCssClasses.Green]);

        var row = content.AddRow().SetMargin(10);

        icon = row.AddImage().SetSize(32).SetMarginRight(5);
        icon.sprite = pinService.PinSprite;

        lblText = row.AddGameLabel();

        RegisterCallback<ClickEvent>(_ => OnClicked());
    }

    public void SetContentTo(PinComponent pin)
    {
        Pin = pin;
        Anchor = pin.Anchor;

        lblText.style.color = icon.tintColor = pin.ColorWithoutAlpha;
        lblText.text = pin.Label;

        style.opacity = pin.ColorAlpha;
    }

}
