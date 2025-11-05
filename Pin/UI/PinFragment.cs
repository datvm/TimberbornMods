using ModSettings.ColorPicker;

namespace Pin.UI;

public class PinFragment(
    ILoc t,
    DialogService diag,
    ColorPickerShower colorPickerShower,
    VisualElementInitializer veInit
) : BaseEntityPanelFragment<PinComponent>
{
#nullable disable
    GameSlider height;
#nullable enable

    protected override void InitializePanel()
    {
        panel.AddGameButtonPadded(t.T("LV.Pin.ChangeLabel"), onClick: ChangeLabel, stretched: true).SetMarginBottom(5);
        panel.AddGameButtonPadded(t.T("LV.Pin.ChangeColor"), onClick: ChangeColor, stretched: true).SetMarginBottom(5);

        panel.AddGameLabel(t.T("LV.Pin.Height"));
        height = panel.AddSlider(values: new(0, 10, 0))
            .RegisterChange(OnHeightChanged)
            .AddEndLabel(v => v.ToString("0.0"));

        panel.Initialize(veInit);
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        height.SetValueWithoutNotify(component!.Height);
    }

    void OnHeightChanged(float value)
    {
        if (!component) { return; }

        component!.Height = value;
        component.UpdatePin();
    }

    async void ChangeLabel()
    {
        if (!component) { return; }

        var text = await diag.PromptAsync(t.T("LV.Pin.LabelPrompt"), component!.Label);
        if (text is null) { return; }

        component.SetEntityName(text);
        component.UpdatePin();
    }

    void ChangeColor()
    {
        if (!component) { return; }

        colorPickerShower.ShowColorPicker(component!.Color, true, OnColorPicked);
    }

    void OnColorPicked(Color color)
    {
        if (!component) { return; }
        component!.Color = color;
        component.UpdatePin();
    }

}
