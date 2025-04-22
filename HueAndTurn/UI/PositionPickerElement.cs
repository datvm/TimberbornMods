namespace HueAndTurn.UI;

public class PositionPickerElement : VisualElement
{
    static readonly ImmutableArray<string> PositionSliderTexts = ["X", "Y", "Z"];
    public static readonly Vector3Int DefaultPosition = Vector3Int.zero;

    public Vector3Int? Position
    {
        get;
        set
        {
            field = value;
            UpdateSliders();
        }
    }
    public event Action<Vector3Int> PositionChanged = delegate { };

    readonly GameSliderInt[] sliders = new GameSliderInt[3];

    public PositionPickerElement()
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            var slider = sliders[i] = this.AddSliderInt(
                label: PositionSliderTexts[i],
                name: "Position-" + i,
                values: new(-50, 50, 0));

            slider.AddEndLabel(i => i.ToString() + "%");

            var index = i;
            slider.RegisterChange(v => ChangePart(index, v));
        }
    }

    public PositionPickerElement SetAxes(int count)
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i].ToggleDisplayStyle(i < count);
        }

        return this;
    }

    public PositionPickerElement RegisterAlternativeManualValue(InputService inputService, ILoc t, VisualElementInitializer veInit, PanelStack panelStack)
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i].RegisterAlternativeManualValue(inputService, t, veInit, panelStack);
        }
        return this;
    }

    public PositionPickerElement RegisterChange(Action<Vector3Int> value)
    {
        PositionChanged += value;
        return this;
    }

    void UpdateSliders()
    {
        var p = Position ?? DefaultPosition;
        for (int i = 0; i < sliders.Length; i++)
        {
            var slider = sliders[i];
            slider.SetValueWithoutNotify(p[i]);
        }
    }

    void ChangePart(int index, int v)
    {
        Position ??= DefaultPosition;

        var position = Position.Value;
        position[index] = v;

        Position = position;
        PositionChanged.Invoke(position);
    }

}
