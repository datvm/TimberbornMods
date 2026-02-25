namespace HueAndTurnX.UI;

public class HueTurnFragment(
    ILoc t,
    ColorPickerShower colorPickerShower,
    VisualElementInitializer veInit,
    GameSliderAlternativeManualValueDI sliderDI,
    IContainer container,
    DialogService diag
) : BaseEntityPanelFragment<HueTurnComponent>
{
    static readonly Color Transparent = new(0, 0, 0, 0);

#nullable disable
    VisualElement color;
    GameSlider txtTransparency;
    SliderGroup txtTranslate, txtRotation, txtScale;
#nullable enable

    protected override void InitializePanel()
    {
        AddColorGroup();
        AddPositioningGroup();
        panel.AddGameButtonPadded(t.T("LV.HTX.ResetAll"), onClick: ResetAll, stretched: true);

        panel.Initialize(veInit);
    }

    void AddColorGroup()
    {
        var colorPanel = panel.AddCollapsiblePanel(t.T("LV.HTX.ColorGroup"), false).SetMarginBottom(10);
        var parent = colorPanel.Container;

        var colorRow = parent.AddRow().AlignItems().SetMarginBottom(10);
        colorRow.AddLabel(t.T("LV.HTX.Color")).SetMarginRight(5);
        color = colorRow.AddChild().SetFlexGrow().SetMarginRight(5);
        color.style.alignSelf = Align.Stretch;
        colorRow.AddGameButtonPadded(t.T("LV.HTX.ColorPick"), PickColor, paddingY: 2).SetMarginRight(5);
        colorRow.AddGameButtonPadded(t.T("LV.HTX.Reset"), ClearColor, paddingY: 2);

        var transRow = parent.AddRow().AlignItems();
        transRow.AddLabel(t.T("LV.HTX.Transparency")).SetMarginRight(5);
        txtTransparency = transRow.AddSlider(values: new(0, 1, 1))
            .RegisterChange(OnTransparencyPicked)
            .AddEndLabel(v => v.ToString("0%"))
            .RegisterAlternativeManualValue(sliderDI)
            .SetFlexGrow();
    }

    void AddPositioningGroup()
    {
        var posPanel = panel.AddCollapsiblePanel(t.T("LV.HTX.PositioningGroup"), false).SetMarginBottom(10);
        var parent = posPanel.Container;

        txtRotation = AddGroup(
            new(-180, 180, 0),
            v => t.T("LV.HTX.Rotation", $"{FD(v.x)}, {FD(v.y)}, {FD(v.z)}"),
            FD,
            v => component!.Positions.SetValues(rotation: v)
        );

        txtTranslate = AddGroup(
            new(-1, 1, 0),
            v => t.T("LV.HTX.Translation", $"{FP(v.x)}, {FP(v.y)}, {FP(v.z)}"),
            FP,
            v => component!.Positions.SetValues(translation: v)
        );

        txtScale = AddGroup(
            new(0.1f, 5, 1),
            v => t.T("LV.HTX.Scale", $"{FP(v.x)}, {FP(v.y)}, {FP(v.z)}"),
            FP,
            v => component!.Positions.SetValues(scale: v)
        );

        SliderGroup AddGroup(SliderValues<float> values, GetSliderGroupTitle titleFunc, Func<float, string> labelFunc, Action<Vector3> onChange)
        {
            var grp = parent.AddChild(() => new SliderGroup(values, titleFunc, labelFunc, sliderDI))
                .SetMarginBottom(10);
            grp.ValueChanged += onChange;
            grp.OnManualSetRequested += () => SetManually(grp, values);
            return grp;
        }
    }

    static string FD(float v) => v.ToString("0") + "°";
    static string FP(float v) => v.ToString("0%");

    async void SetManually(SliderGroup grp, SliderValues<float> valuesRange)
    {
        var diag = container.GetInstance<ManualValueDialog>();
        var values = await diag.ShowAsync(grp.Values, valuesRange);

        if (values is not null)
        {
            grp.SetValues(values.Value, raiseValueChanged: true);
        }
    }

    void OnTransparencyPicked(float a)
    {
        component!.Colors.SetValues(transparency: a);
    }

    void PickColor()
    {
        var comp = component!.Colors;
        var initial = comp.Color ?? Color.white;

        colorPickerShower.ShowColorPicker(initial, true, c =>
        {
            comp.SetValues(color: c);
            RefreshValues();
        });
    }

    void ClearColor()
    {
        component!.Colors.ClearColor();
        RefreshValues();
    }

    async void ResetAll()
    {
        if (!await diag.ConfirmAsync(t.T("LV.HTX.ResetAllConfirm"))) { return; }

        component!.Clear();
        RefreshValues();
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (component is null) { return; }

        RefreshValues();
    }

    void RefreshValues()
    {
        var coloring = component!.Colors;
        color.style.backgroundColor = coloring.Color ?? Transparent;
        txtTransparency.SetValueWithoutNotify(coloring.Transparency ?? 1);

        var positioning = component!.Positions;
        txtTranslate.SetValues(positioning.Translation ?? default);
        txtRotation.SetValues(positioning.Rotation ?? default);
        txtScale.SetValues(positioning.Scale ?? Vector3.one);
    }

}
