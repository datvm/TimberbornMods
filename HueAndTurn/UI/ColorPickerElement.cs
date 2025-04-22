namespace HueAndTurn.UI;

public class ColorPickerElement : VisualElement
{
    public static readonly Color DefaultColor = new(1f, 1f, 1f, .5f);
    static readonly ImmutableArray<string> ColorSliderTexts = ["R", "G", "B", "%"];

    public Color Color
    {
        get;
        set
        {
            field = value;
            UpdateSliders();
        }
    }
    public event Action<Color> ColorChanged = delegate { };


    readonly GameSliderInt[] sliders;

    public ColorPickerElement()
    {
        sliders = new GameSliderInt[ColorSliderTexts.Length];
        for (int i = 0; i < sliders.Length; i++)
        {
            var z = i;

            var slider = sliders[i] = this.AddSliderInt(
                label: ColorSliderTexts[i],
                name: "Color-" + i,
                values: new(0, 255, 255))
                
                .AddEndLabel(i => i.ToString())
                .RegisterChange(v => ChangePart(z, v));
        }

        Color = DefaultColor;
    }

    public ColorPickerElement RegisterAlternativeManualValue(InputService inputService, ILoc t, VisualElementInitializer veInit, PanelStack panelStack)
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i].RegisterAlternativeManualValue(inputService, t, veInit, panelStack);
        }

        return this;
    }

    public ColorPickerElement RegisterChange(Action<Color> action)
    {
        ColorChanged += action;
        return this;
    }

    void UpdateSliders()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            var slider = sliders[i];
            slider.SetValueWithoutNotify(Mathf.RoundToInt(Color[i] * 255));
        }
    }

    void ChangePart(int index, int v)
    {
        var color = Color;
        color[index] = v / 255f;

        Color = color;
        ColorChanged.Invoke(color);
    }

}
