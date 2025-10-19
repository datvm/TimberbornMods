namespace ColorfulZipline.UI;

public class ColorfulZiplineConnectionPanel : CollapsiblePanel
{
    readonly ColorPickerShower colorPickerShower;

    public event Action<ZiplineCableColor>? OnColorSet;
    public event Action<ZiplineCableColor>? OnApplyBuildingRequested;
    public event Action<ZiplineCableColor>? OnApplyToAllRequested;

    readonly VisualElement leftColor, rightColor;

    public ZiplineCableColor Color { get; private set; }

    public ColorfulZiplineConnectionPanel(ILoc t, ColorPickerShower colorPickerShower)
    {
        this.colorPickerShower = colorPickerShower;
        this.SetMarginBottom();

        var row = Container.AddRow().AlignItems().SetMarginBottom(10);
        leftColor = CreateColorElement(true);
        rightColor = CreateColorElement(false);

        row.AddGameButton(t.T("LV.CZ.Reset"), onClick: OnReset)
            .SetPadding(5, 5)
            .SetFlexShrink(0);

        Container.AddGameLabel(t.T("LV.CZ.ApplyAll"));
        Container.AddGameButton(t.T("LV.CZ.InBuilding"), () => OnApplyBuildingRequested?.Invoke(Color))
            .SetPadding(5, 5)
            .SetMarginBottom(5);
        Container.AddGameButton(t.T("LV.CZ.All"), () => OnApplyToAllRequested?.Invoke(Color))
            .SetPadding(5, 5);

        SetExpand(false);

        VisualElement CreateColorElement(bool left)
        {
            var colorRow = row.AddChild();
            var s = colorRow.style;
            s.flexBasis = Length.Percent(50);
            s.alignSelf = Align.Stretch;

            colorRow.RegisterCallback<ClickEvent>(_ => OnColorPickerRequested(left));

            return colorRow;
        }
    }

    public void SetColor(ZiplineCableColor color)
    {
        Color = color;
        leftColor.style.backgroundColor = color.Left;
        rightColor.style.backgroundColor = color.Right;
    }

    void OnReset() => PickColor(ZiplineCableColor.Default);
    void OnColorPickerRequested(bool left) => colorPickerShower.ShowColorPicker(
        left ? Color.Left : Color.Right,
        false,
        c => OnColorPicked(left, c));

    void OnColorPicked(bool left, Color color)
    {
        var newColor = left
            ? Color with { Left = color }
            : Color with { Right = color };
        PickColor(newColor);
    }

    void PickColor(ZiplineCableColor color)
    {
        SetColor(color);
        OnColorSet?.Invoke(color);
    }

}
