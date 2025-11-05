namespace RealLights.UI;

public class RealLightsFragment(
    VisualElementInitializer initializer,
    ILoc t,
    DevModeManager devs,
    InputService input,
    PanelStack panelStack,
    ColorPickerShower colorPickerShower
) : IEntityPanelFragment, IInputProcessor
{
    const string CopyKeyId = "CopyLightColor";
    const string PasteKeyId = "PasteLightColor";

    EntityPanelFragmentElement panel = null!;
    RealLightsComponent? realLight;

    Toggle chkForceOffAll = null!;
    Toggle chkForceOff = null!;
    Toggle chkForceNightLight = null!;
    VisualElement lightConfigs = null!;
    VisualElement devPanel = null!;

    RealLightClipboard? clipboard;

    ImmutableArray<GameSliderInt> devPositions;

    public void ClearFragment()
    {
        input.RemoveInputProcessor(this);
        panel.Visible = false;
        lightConfigs.Clear();
        realLight = null;        
    }

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Visible = false,
        };

        var scroll = panel.AddGameScrollView().SetMaxHeight(200);
        
        chkForceOffAll = scroll.AddToggle(t.T("LV.RL.ForceOffAll"), onValueChanged: OnForceOffPrefabChanged)
            .SetMarginBottom(5);
        chkForceOff = scroll.AddToggle(t.T("LV.RL.ForceOff"), onValueChanged: OnForceOffChanged)
            .SetMarginBottom(5);
        chkForceNightLight = scroll.AddToggle(t.T("LV.RL.ForceNightLight"), onValueChanged: OnForceNightLightChanged)
            .SetMarginBottom(10);

        lightConfigs = scroll.AddChild().SetMarginBottom();

        scroll.AddGameButton(t.T("LV.RL.Reset"), OnReset, "ResetColor", stretched: true)
            .SetFlexGrow();

        AddDevPanel(scroll);

        return this.panel.Initialize(initializer);
    }

    void AddDevPanel(VisualElement parent)
    {
        devPanel = parent.AddChild().SetMargin(top: 20);

        devPanel.AddGameButton(
            "Toggle Light positions indicators",
            () => realLight?.ToggleDebugLight(!realLight.IsDrawingDebugLight), stretched: true);

        devPanel.AddLabel("First light position:");
        devPositions = [
            AddPositionSlider("X", (v, c) => c with { x = v }),
            AddPositionSlider("Y", (v, c) => c with { y = v }),
            AddPositionSlider("Z", (v, c) => c with { z = v }),
        ];

        GameSliderInt AddPositionSlider(string name, Func<float, Vector3, Vector3> getValue)
        {
            var slider = devPanel.AddSliderInt(name, values: new(-100, 100, 0))
                .AddEndLabel(v => (v / 10f).ToString())
                .RegisterChangeCallback(ev =>
                {
                    var curr = realLight!.FirstLightPosition;
                    realLight.SetLightPosition(getValue(ev.newValue / 10f, curr));
                })
                .RegisterAlternativeManualValue(input, t, initializer, panelStack);

            return slider;
        }
    }

    public void ShowFragment(BaseComponent entity)
    {
        realLight = entity.GetComponent<RealLightsComponent>();
        if (realLight is null || !realLight.HasRealLight) { return; }

        UpdatePanelValues();
        input.AddInputProcessor(this);
    }

    public void OnForceOffPrefabChanged(bool enabled)
    {
        realLight?.SetForceOffPrefab(enabled);
        SetToggleUi();
    }

    void OnForceNightLightChanged(bool on)
    {
        realLight?.SetForceNightLightOn(on);
    }

    void OnForceOffChanged(bool on)
    {
        realLight?.SetForceOff(on);
        SetToggleUi();
    }

    void OnLightCustomSet(int index, CustomRealLightProperties props)
    {
        realLight?.SetCustomProperties(index, props);
    }

    void OnReset()
    {
        realLight?.Reset();
        UpdatePanelValues();
    }

    void UpdatePanelValues()
    {
        chkForceOff.value = realLight!.ForceOff;
        
        chkForceOffAll.value = realLight!.ForcedOffPrefab;
        chkForceOffAll.text = t.T("LV.RL.ForceOffAll", realLight.BuildingName);

        chkForceNightLight.ToggleDisplayStyle(realLight.HasNightLight);
        chkForceNightLight.value = realLight.ForceNightLightOn;

        SetToggleUi();

        var lightCount = realLight.Spec!.Lights.Length;

        lightConfigs.Clear();
        for (int i = 0; i < lightCount; i++)
        {
            var z = i;
            var l = realLight.Spec.Lights[i];
            var props = realLight.GetLightProperties(i);

            var config = lightConfigs.AddChild().SetMarginBottom(10);

            if (lightCount > 1)
            {
                config.AddGameLabel(t.T("LV.RL.LightConfig", i + 1));
            }

            config.AddSliderInt(label: t.T("LV.RL.Range"), values: new(0, 20, (int)props.Range))
                .AddEndLabel(v => v.ToString())
                .RegisterAlternativeManualValue(input, t, initializer, panelStack)
                .RegisterChangeCallback(ev => OnLightCustomSet(z, new() { Range = ev.newValue }));

            config.AddSliderInt(label: t.T("LV.RL.Intensity"), values: new(0, 20, (int)props.Intensity))
                .AddEndLabel(v => v.ToString())
                .RegisterAlternativeManualValue(input, t, initializer, panelStack)
                .RegisterChangeCallback(ev => OnLightCustomSet(z, new() { Intensity = ev.newValue }));

            config.AddGameButtonPadded(t.T("LV.RL.ChangeColor"), onClick: () => OnColorRequested(z));
        }
        lightConfigs.Initialize(initializer);

        var pos = realLight.FirstLightPosition;
        for (int i = 0; i < 3; i++)
        {
            devPositions[i].SetValue((int)(pos[i] * 10));
        }

        devPanel.ToggleDisplayStyle(devs.Enabled);

        panel.Visible = true;
    }

    void OnColorRequested(int index)
    {
        if (!realLight) { return; }

        var props = realLight!.GetLightProperties(index);
        var currColor = props.Color;

        colorPickerShower.ShowColorPicker(
            currColor,
            false, 
            color => realLight.SetCustomColor(index, color));
    }

    public void UpdateFragment() { }

    public bool ProcessInput()
    {
        if (realLight is null) { return false; }

        if (input.IsKeyHeld(CopyKeyId))
        {
            clipboard = realLight.ToClipboard();
            return true;
        }
        else if (input.IsKeyHeld(PasteKeyId))
        {
            if (clipboard is null) { return false; }

            realLight.ImportClipboard(clipboard.Value);
            UpdatePanelValues();
            return true;
        }

        return false;
    }

    void SetToggleUi()
    {
        chkForceOff.enabledSelf = !chkForceOffAll.value;
        chkForceNightLight.enabledSelf = !chkForceOffAll.value && !chkForceOff.value;
    }

}

