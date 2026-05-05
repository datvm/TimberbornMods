using ModSettings.ColorPicker;

namespace DynamicTailsBanners.UI;

[BindTransient]
public class ColorPickerPanel : VisualElement
{

    public Color Color { get; private set; } = Color.white;
    public event EventHandler<Color>? OnColorChanged;

    readonly Label lbl;
    readonly VisualElement preview;
    readonly ColorPickerShower colorPickerShower;

    public ColorPickerPanel(ColorPickerShower colorPickerShower, ILoc t)
    {
        this.colorPickerShower = colorPickerShower;

        this.SetAsRow().AlignItems();

        lbl = this.AddLabel(t.T("LV.DTB.ColorLabel")).SetMarginRight(5);
        preview = this.AddChild().SetFlexGrow().SetMarginRight(5);
        preview.style.alignSelf = Align.Stretch;

        this.AddGameButtonPadded(t.T("LV.DTB.Pick"), PickColor);
    }

    public void SetLabel(string text) => lbl.text = text;

    void PickColor()
    {
        colorPickerShower.ShowColorPicker(Color, false, SetColor);
    }

    public void SetColor(Color color)
    {
        if (InternalSetColor(color))
        {
            OnColorChanged?.Invoke(this, color);
        }
    }

    public void SetColorWithoutNotifying(Color color) => InternalSetColor(color);

    bool InternalSetColor(Color color)
    {
        if (Color == color)
        {
            return false;
        }

        Color = color;
        preview.style.backgroundColor = color;
        return true;
    }

}
