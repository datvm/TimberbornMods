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
            var slider = sliders[i] = this.AddSliderInt(
                label: ColorSliderTexts[i],
                name: "Color-" + i,
                values: new(0, 255, 255));

            slider.AddEndLabel(i => i.ToString());

            var index = i;
            slider.RegisterChange(v => ChangePart(index, v));
        }
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
            slider.SetValue(Mathf.RoundToInt(Color[i] * 255));
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
