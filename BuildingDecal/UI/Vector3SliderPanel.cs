namespace BuildingDecal.UI;

public class Vector3SliderPanel : CollapsiblePanel
{

    public event Action<Vector3>? OnValueChanged;

    readonly Label[] labels;
    readonly GameSlider[] sliders;

    Func<Vector3, string>? titleFunc;

    public Vector3 Value
    {
        get => new(sliders[0].Value, sliders[1].Value, sliders[2].Value);
        set
        {
            for (int i = 0; i < 3; i++)
            {
                sliders[i].Value = value[i];
            }
        }
    }

    public Vector3SliderPanel(GameSliderAlternativeManualValueDI di)
    {
        var parent = Container;

        this.SetMarginBottom(5);

        sliders = new GameSlider[3];
        labels = new Label[3];
        for (int i = 0; i < 3; i++)
        {
            labels[i] = parent.AddLabel();
            var slider = sliders[i] = parent.AddSlider()
                .AddEndLabel(v => v.ToString("0.00"))
                .RegisterChange(v => InternalOnValueChanged(true))
                .RegisterAlternativeManualValue(di);
        }

        SetExpand(false);
    }

    public Vector3SliderPanel SetRange(float min, float max)
    {
        foreach (var s in sliders)
        {
            s.Slider.lowValue = min;
            s.Slider.highValue = max;
        }

        return this;
    }

    public Vector3SliderPanel SetLabels(string[] labels)
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            if (labels.Length > i)
            {
                this.labels[i].text = labels[i];
            }
            else
            {
                sliders[i].SetDisplay(false);
                this.labels[i].SetDisplay(false);
            }

        }
        return this;
    }

    public new Vector3SliderPanel SetTitle(string title)
    {
        base.SetTitle(title);
        return this;
    }

    public Vector3SliderPanel RegisterChange(Action<Vector3> callback)
    {
        OnValueChanged += callback;
        return this;
    }

    void InternalOnValueChanged(bool notify)
    {
        var v = Value;

        if (titleFunc is not null)
        {
            SetTitle(titleFunc(v));
        }

        if (notify)
        {
            OnValueChanged?.Invoke(v);
        }
    }

    public Vector3SliderPanel SetValueWithoutNotify(Vector3 value)
    {
        for (int i = 0; i < 3; i++)
        {
            sliders[i].SetValueWithoutNotify(value[i]);
        }

        InternalOnValueChanged(false);
        return this;
    }

    public Vector3SliderPanel SetTitleFunction(Func<Vector3, string> func)
    {
        titleFunc = func;
        SetTitle(titleFunc(Value));
        return this;
    }

}
