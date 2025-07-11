namespace HueAndTurn.UI;
public class HueAndTurnFragment(
    VisualElementInitializer initializer,
    ILoc t,
    InputService input,
    HueAndTurnMassApplier massApplier,
    PanelStack panelStack
) : IEntityPanelFragment, IInputProcessor
{
    const string CopyId = "CopyHueAndTurn";
    const string PasteId = "PasteHueAndTurn";

    ReadOnlyHueAndTurnProperties? clipboard;
    IHueAndTurnComponent? comp;
    
    bool internalSet;

#nullable disable
    EntityPanelFragmentElement panel;
    
    Toggle chkColor;    
    ColorPickerElement colorPicker;
    ConfigGroup grpFluidColor;
    Toggle chkFluidColor;
    ColorPickerElement fluidColorPicker;

    GameSliderInt transparencySlider;
    GameSliderInt rotationSlider;
    GameSliderInt rotationXSlider;
    GameSliderInt rotationZSlider;
    PositionPickerElement rotationPivotPicker;
    PositionPickerElement translatePicker;
    PositionPickerElement scalePicker;

    ConfigGroup grpApplyAll;
#nullable enable

    public void ClearFragment()
    {
        input.RemoveInputProcessor(this);
        panel.Visible = false;
        comp = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Background = EntityPanelFragmentBackground.PurpleStriped,
            Visible = false
        };

        panel.AddGameLabel(t.T("LV.HNT.Title").Bold()).SetMarginBottom(10);

        AddColorGroup(panel);
        grpFluidColor = AddFluidColorGroup(panel);
        AddRotationGroup(panel);
        AddPositioningGroup(panel);
        grpApplyAll = AddApplyAllGroup(panel);

        panel.AddGameButton(t.T("LV.HNT.Reset"), Reset, "ResetColor", stretched: true)
            .SetFlexGrow();

        return panel.Initialize(initializer);
    }

    void AddColorGroup(VisualElement parent)
    {
        var grp = parent.AddChild<ConfigGroup>()
            .SetHeader("LV.HNT.Color".T(t));
        var container = grp.Content;

        chkColor = container.AddToggle(t.T("LV.HNT.Color"),
            onValueChanged: OnColorToggled);

        colorPicker = container.AddChild<ColorPickerElement>()
            .RegisterAlternativeManualValue(input, t, initializer, panelStack)
            .RegisterChange(OnColorPicked);

        transparencySlider = container.AddSliderInt(
            t.T("LV.HNT.Transparency"),
            values: new(0, 100, 100))
            .AddEndLabel(v => v + "%")
            .RegisterAlternativeManualValue(input, t, initializer, panelStack)
            .RegisterChange(SetTransparency);
    }

    ConfigGroup AddFluidColorGroup(VisualElement parent)
    {
        var grp = parent.AddChild<ConfigGroup>()
            .SetHeader("LV.HNT.FluidColor".T(t))
            .SetDisplay(false);
        var container = grp.Content;

        chkFluidColor = container.AddToggle(t.T("LV.HNT.FluidColor"),
            onValueChanged: OnFluidColorToggled);

        fluidColorPicker = container.AddChild<ColorPickerElement>()
            .RegisterAlternativeManualValue(input, t, initializer, panelStack)
            .RegisterChange(OnFluidColorPicked);

        return grp;
    }

    void AddRotationGroup(VisualElement parent)
    {
        var grp = parent.AddChild<ConfigGroup>()
            .SetHeader("LV.HNT.Rotation".T(t));
        var container = grp.Content;

        rotationSlider = AddRotationPicker(container, "LV.HNT.Rotation", SetRotation);
        rotationXSlider = AddRotationPicker(container, "LV.HNT.RotationX", v => SetAdvRotation(true, v));
        rotationZSlider = AddRotationPicker(container, "LV.HNT.RotationZ", v => SetAdvRotation(false, v));

        container.AddGameLabel(t.T("LV.HNT.RotationPivot")).SetMarginBottom(5);
        rotationPivotPicker = container.AddChild<PositionPickerElement>()
            .SetAxes(2)
            .RegisterChange(SetRotationPivot)
            .RegisterAlternativeManualValue(input, t, initializer, panelStack)
            .SetMarginBottom(10);

        GameSliderInt AddRotationPicker(VisualElement parent, string text, Action<int> onChange)
        {
            return parent.AddSliderInt(t.T(text), values: new(-180, 180, 0))
                .RegisterChange(onChange)
                .RegisterAlternativeManualValue(input, t, initializer, panelStack)
                .AddEndLabel(v => v + "°")
                .SetMarginBottom(10);
        }
    }

    void AddPositioningGroup(VisualElement parent)
    {
        var grp = parent.AddChild<ConfigGroup>()
            .SetHeader("LV.HNT.Positioning".T(t));
        var container = grp.Content;

        container.AddGameLabel(t.T("LV.HNT.Translate")).SetMarginBottom(5);
        translatePicker = container.AddChild<PositionPickerElement>()
            .RegisterChange(SetTranslation)
            .RegisterAlternativeManualValue(input, t, initializer, panelStack)
            .SetMarginBottom(10);

        container.AddGameLabel(t.T("LV.HNT.Scale")).SetMarginBottom(5);
        scalePicker = container.AddChild<PositionPickerElement>()
            .RegisterChange(SetScale)
            .RegisterAlternativeManualValue(input, t, initializer, panelStack)
            .SetMarginBottom(10);
    }

    ConfigGroup AddApplyAllGroup(VisualElement parent)
    {
        var grp = parent.AddChild<ConfigGroup>()
            .SetHeader("LV.HNT.ApplyAll".T(t));
        var container = grp.Content;

        AddApplyAllButton("LV.HNT.ColorAll", massApplier.CopyColor);
        AddApplyAllButton("LV.HNT.ApplyAll", massApplier.CopyAllProps);
        AddApplyAllButton("LV.HNT.RotateRandomAll", massApplier.RandomizeRotations);
        AddApplyAllButton("LV.HNT.ResetAll", massApplier.Reset);

        return grp;

        void AddApplyAllButton(string key, Action<IHueAndTurnComponent> action)
        {
            container.AddGameButton(t.T(key), () => ApplyAll(action), stretched: true)
                .SetMarginBottom(5)
                .SetPadding(paddingY: 5);
        }
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = TryGetComponent(entity);
        if (comp is null)
        {
            panel.Visible = false;
            return;
        }

        var label = entity.GetComponentFast<LabeledEntity>();
        grpApplyAll.SetHeader(t.T("LV.HNT.ToAll", label.DisplayName));

        UpdatePanelContent();
        input.AddInputProcessor(this);
    }

    static IHueAndTurnComponent TryGetComponent(BaseComponent entity)
    {

        if (MStarter.HasMacroManagement)
        {
            var mm = MMHueAndTurnComponent.TryGetMM(entity);
            if (mm is not null)
            {
                return mm;
            }
        }

        return entity.GetComponentFast<HueAndTurnComponent>();
    }

    public void UpdateFragment() { }

    void ApplyAll(Action<IHueAndTurnComponent> action)
    {
        if (comp is null) { return; }

        massApplier.Confirm(() => action(comp));

        UpdatePanelContent();
    }

    void OnColorToggled(bool hasColor)
    {
        OnColorValueChanged(hasColor ? colorPicker.Color : null);
    }

    void OnColorPicked(Color color)
    {
        if (!chkColor.value) { return; }

        OnColorValueChanged(color);
    }

    void OnColorValueChanged(Color? color)
    {
        if (internalSet || comp is null) { return; }

        comp.SetColor(color);
        UpdateColorEnabled();
    }

    void OnFluidColorToggled(bool hasColor) => OnFluidColorChanged(hasColor ? fluidColorPicker.Color : null);

    void OnFluidColorPicked(Color color)
    {
        if (!chkFluidColor.value) { return; }
        OnFluidColorChanged(color);
    }

    void OnFluidColorChanged(Color? color)
    {
        if (internalSet || comp is null) { return; }

        comp.SetFluidColor(color);
        UpdateColorEnabled();
    }

    void SetTransparency(int transparency)
    {
        if (internalSet || comp is null) { return; }

        comp.SetTransparency(transparency);
    }

    void SetRotation(int rotation)
    {
        if (internalSet || comp is null) { return; }

        comp.SetRotation(rotation);
    }

    void SetAdvRotation(bool isX, int rotation)
    {
        if (internalSet || comp is null) { return; }

        var curr = comp.Properties.RotationXZ ?? default;
        var rotationXZ = isX ? (curr with { x = rotation }) : (curr with { y = rotation });

        comp.SetRotationXZ(rotationXZ);
    }

    void SetRotationPivot(Vector3Int pivot)
    {
        if (internalSet || comp is null) { return; }

        comp.SetRotationPivot((Vector2Int)pivot);
    }

    void SetTranslation(Vector3Int translation)
    {
        if (internalSet || comp is null) { return; }

        comp.SetTranslation(translation);
    }

    void SetScale(Vector3Int scale)
    {
        if (internalSet || comp is null) { return; }
        
        comp.SetScale(scale);
    }

    void Reset()
    {
        if (comp is null) { return; }

        internalSet = true;
        comp.Reset();
        UpdatePanelContent();
    }

    void UpdatePanelContent()
    {
        if (comp is null) { return; }
        internalSet = true;
        var props = comp.Properties;

        chkColor.SetValueWithoutNotify(props.Color is not null);
        if (props.Color is not null)
        {
            colorPicker.Color = props.Color.Value;
        }
        transparencySlider.SetValueWithoutNotify(props.Transparency ?? 100);
        transparencySlider.SetEnabled(comp.CanHaveTransparency);

        //grpFluidColor.SetDisplay(comp.HasFluid);
        //chkFluidColor.SetValueWithoutNotify(props.FluidColor is not null);
        //if (props.FluidColor is not null)
        //{
        //    fluidColorPicker.Color = props.FluidColor.Value;
        //}

        rotationSlider.SetValueWithoutNotify(props.Rotation ?? 0);
        rotationPivotPicker.Position = props.RotationPivot?.ToVector3Int(0) ?? null;
        rotationPivotPicker.SetEnabled(comp.RotationPivotSupported);

        var rotationXZ = props.RotationXZ ?? default;
        rotationXSlider.SetValueWithoutNotify(rotationXZ.x);
        rotationZSlider.SetValueWithoutNotify(rotationXZ.y);

        translatePicker.Position = props.Translation;
        scalePicker.Position = props.Scale;

        UpdateColorEnabled();
        panel.Visible = true;
        internalSet = false;
    }

    void UpdateColorEnabled()
    {
        colorPicker.enabledSelf = chkColor.value;
        fluidColorPicker.enabledSelf = chkFluidColor.value;
    }

    public bool ProcessInput()
    {
        if (comp is null) { return false; }

        if (input.IsKeyHeld(CopyId))
        {
            clipboard = comp.Properties;
            return true;
        }
        else if (input.IsKeyHeld(PasteId))
        {
            if (clipboard is null) { return false; }

            comp.ApplyProperties(clipboard.Value);
            return true;
        }

        return false;
    }

}
