namespace HueAndTurnX.UI;

public delegate string GetSliderGroupTitle(Vector3 value);
public class SliderGroup : CollapsiblePanel
{
    public static readonly string[] DefaultLabels = ["X", "Y", "Z"];

    readonly GetSliderGroupTitle titleFunc;

    public Vector3 Values { get; private set; }

    readonly GameSlider[] sliders = new GameSlider[3];

    public event Action<Vector3> ValueChanged = null!;
    public event Action OnManualSetRequested = null!;

    public SliderGroup(
        SliderValues<float> values,
        GetSliderGroupTitle titleFunc,
        Func<float, string> endLabelFunc,
        GameSliderAlternativeManualValueDI sliderDI
    )
    {
        this.titleFunc = titleFunc;
        
        var parent = Container;
        var t = sliderDI.t;

        for (int i = 0; i < sliders.Length; i++)
        {
            var row = parent.AddRow().AlignItems().SetMarginBottom(5);
            row.AddLabel(DefaultLabels[i] + ": ").SetMarginRight(5);

            sliders[i] = row.AddSlider(values: values)
                .RegisterChange(_ => OnSliderChanged())
                .AddEndLabel(endLabelFunc)
                .RegisterAlternativeManualValue(sliderDI)
                .SetFlexGrow();
        }

        var btnRow = parent.AddRow();
        btnRow.AddGameButtonPadded(t.T("LV.HTX.SetManually"), onClick: () => OnManualSetRequested(), paddingY: 2)
            .SetFlexGrow().SetMarginRight(5);
        btnRow.AddGameButtonPadded(t.T("LV.HTX.Reset"), onClick: () => Reset(values.Default), paddingY: 2);
    }

    void Reset(float v)
    {
        SetValues(new(v, v, v), raiseValueChanged: true);
    }

    void OnSliderChanged()
    {
        Values = new(sliders[0].Value, sliders[1].Value, sliders[2].Value);
        ValueChanged(Values);
        UpdateDisplay();
    }

    public void SetValues(Vector3 values, bool raiseValueChanged = false)
    {
        Values = values;
        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i].SetValueWithoutNotify(values[i]);
        }
        UpdateDisplay();

        if (raiseValueChanged)
        {
            ValueChanged(Values);
        }
    }

    void UpdateDisplay()
    {
        SetTitle(titleFunc(Values));
    }


}
