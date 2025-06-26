namespace HydroFormaProjects.UI;

public class WaterHeightSlider : VisualElement
{

    readonly GameSliderInt slider;
    readonly Label lblHeight;

    const float HeightPerUnit = .05f;

    public float Height
    {
        get => UnitsToHeight(slider.Value);
        set
        {
            value = Mathf.Clamp(value, 0, MaxHeight);
            slider.SetValueWithoutNotify(HeightToUnits(value));
        }
    }

    public WaterHeightSlider()
    {
        slider = this.AddSliderInt().SetWidthPercent(100);
        lblHeight = this.AddGameLabel("0");
        lblHeight.style.unityTextAlign = TextAnchor.MiddleCenter;

        slider.RegisterChange(_ => ReloadLabel());
    }

    public WaterHeightSlider SetLabelVisible(bool visible)
    {
        lblHeight.SetDisplay(visible);
        return this;
    }

    public float MaxHeight { get; private set; } = 1f;

    public WaterHeightSlider RegisterHeightChange(Action<float> callback)
    {
        slider.RegisterChange(value => callback(UnitsToHeight(value)));
        return this;
    }

    public void SetValues(float curr, float maxHeight)
    {
        curr = Mathf.Clamp(curr, 0, maxHeight);

        var s = slider.Slider;
        MaxHeight = maxHeight;
        s.SetHighValueWithoutNotify(HeightToUnits(maxHeight));
        s.SetValueWithoutNotify(HeightToUnits(curr));
        ReloadLabel();
    }

    void ReloadLabel() => lblHeight.text = Height.ToString("0.00");

    static int HeightToUnits(float height)
    {
        return Mathf.RoundToInt(height / HeightPerUnit);
    }

    static float UnitsToHeight(int units)
    {
        return units * HeightPerUnit;
    }

}
