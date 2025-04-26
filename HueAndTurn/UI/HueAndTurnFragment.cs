global using Timberborn.InputSystem;

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

    HueAndTurnProperties? clipboard;
    HueAndTurnComponent? comp;

    bool internalSet;

#nullable disable
    EntityPanelFragmentElement panel;
    Toggle chkColor;
    ColorPickerElement colorPicker;
    GameSliderInt rotationSlider;
    PositionPickerElement rotationPivotPicker;
    PositionPickerElement translatePicker;
    VisualElement advOptionsPanel;
    Label lblResetAll;
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

        AddColorPicker(panel);
        AddRotationPicker(panel);

        panel.AddGameButton(t.T("LV.HNT.Reset"), Reset, "ResetColor", stretched: true)
            .SetMargin(top: 20, bottom: 20)
            .SetFlexGrow();

        AddAdvancedOptions(panel);

        return panel.Initialize(initializer);
    }

    void AddColorPicker(VisualElement parent)
    {
        var container = parent.AddChild();

        chkColor = container.AddToggle(t.T("LV.HNT.Color"),
            onValueChanged: OnColorToggled);

        colorPicker = container.AddChild<ColorPickerElement>()
            .RegisterAlternativeManualValue(input, t, initializer, panelStack)
            .RegisterChange(OnColorPicked);
    }

    void AddRotationPicker(VisualElement parent)
    {
        rotationSlider = parent.AddSliderInt(t.T("LV.HNT.Rotation"), values: new(-180, 180, 0));
        rotationSlider
            .RegisterChange(SetRotation)
            .RegisterAlternativeManualValue(input, t, initializer, panelStack)
            .AddEndLabel(v => v + "°");
    }

    void AddAdvancedOptions(VisualElement parent)
    {
        panel.AddGameButton(t.T("LV.HNT.Advanced"), ToggleAdv, stretched: true)
            .SetMarginBottom(10);

        advOptionsPanel = parent.AddChild(name: "AdvancedOptions");

        advOptionsPanel.AddGameLabel(t.T("LV.HNT.RotationPivot")).SetMarginBottom(5);
        rotationPivotPicker = advOptionsPanel.AddChild<PositionPickerElement>()
            .SetAxes(2)
            .RegisterChange(SetRotationPivot)
            .RegisterAlternativeManualValue(input, t, initializer, panelStack)
            .SetMarginBottom();

        advOptionsPanel.AddGameLabel(t.T("LV.HNT.Translate")).SetMarginBottom(5);
        translatePicker = advOptionsPanel.AddChild<PositionPickerElement>()
            .RegisterChange(SetTranslation)
            .RegisterAlternativeManualValue(input, t, initializer, panelStack)
            .SetMarginBottom(10);

        AddApplyAllOptions(advOptionsPanel);

        ToggleAdv(); // Hide it
    }

    void AddApplyAllOptions(VisualElement parent)
    {
        var container = parent.AddChild();

        lblResetAll = container.AddGameLabel("").SetMarginBottom(5);

        AddApplyAllButton("LV.HNT.ColorAll", massApplier.CopyColor);
        AddApplyAllButton("LV.HNT.ApplyAll", massApplier.CopyAllProps);
        AddApplyAllButton("LV.HNT.RotateRandomAll", massApplier.RandomizeRotations);
        AddApplyAllButton("LV.HNT.ResetAll", massApplier.Reset);

        void AddApplyAllButton(string key, Action<HueAndTurnComponent> action)
        {
            container.AddGameButton(t.T(key), () => ApplyAll(action), stretched: true);
        }
    }

    void ToggleAdv()
    {
        advOptionsPanel.ToggleDisplayStyle(!advOptionsPanel.IsDisplayed());
    }

    public void ShowFragment(BaseComponent entity)
    {
        comp = entity.GetComponentFast<HueAndTurnComponent>();
        if (comp is null)
        {
            panel.Visible = false;
            return;
        }

        
        lblResetAll.text = t.T("LV.HNT.ToAll", comp.PrefabName);

        UpdatePanelContent();
        input.AddInputProcessor(this);
    }

    public void UpdateFragment() { }

    void ApplyAll(Action<HueAndTurnComponent> action)
    {
        if (!comp) { return; }

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
        if (internalSet || !comp) { return; }

        comp.Properties.Color = color;
        comp.ApplyColor();

        UpdateColorEnabled();
    }

    void SetRotation(int rotation)
    {
        if (internalSet || !comp) { return; }

        comp.Properties.Rotation = rotation;
        comp.ApplyRepositioning();
    }

    void SetRotationPivot(Vector3Int pivot)
    {
        if (internalSet || !comp) { return; }

        comp.Properties.RotationPivot = (Vector2Int)pivot;
        comp.ApplyRepositioning();
    }

    void SetTranslation(Vector3Int translation)
    {
        if (internalSet || !comp) { return; }

        comp.Properties.Translation = translation;
        comp.ApplyRepositioning();
    }

    void Reset()
    {
        if (!comp) { return; }

        internalSet = true;
        comp.Reset();
        UpdatePanelContent();
    }

    void UpdatePanelContent()
    {
        if (!comp) { return; }
        internalSet = true;
        var props = comp.Properties;

        chkColor.value = props.Color is not null;
        if (props.Color is not null)
        {
            colorPicker.Color = props.Color.Value;
        }

        rotationSlider.SetValueWithoutNotify(props.Rotation ?? 0);
        rotationPivotPicker.Position = props.RotationPivot?.ToVector3Int(0) ?? null;
        rotationPivotPicker.SetEnabled(comp.RotationPivotSupported);
        translatePicker.Position = props.Translation;

        UpdateColorEnabled();
        panel.Visible = true;
        internalSet = false;
    }

    void UpdateColorEnabled()
    {
        colorPicker.enabledSelf = chkColor.value;
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

            comp.ApplyProperties(clipboard);
            return true;
        }

        return false;
    }

}
