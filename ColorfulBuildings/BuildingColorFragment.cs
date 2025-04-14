global using Timberborn.InputSystem;

namespace ColorfulBuildings;

public class BuildingColorFragment(VisualElementInitializer initializer, ILoc t, InputService input) : IEntityPanelFragment, IInputProcessor
{
    const string CopyColorKeyId = "CopyColor";
    const string PasteColorKeyId = "PasteColor";

    Vector3Int? clipboard;

    EntityPanelFragmentElement panel = null!;
    BuildingColorComponent? buildingColor;

    static readonly string[] SliderTexts = ["R", "G", "B"];
    readonly GameSliderInt[] sliders = new GameSliderInt[3];
    GameSliderInt rotationSlider = null!;

    static readonly Vector3Int DefaultColor = new(255, 255, 255);

    public void ClearFragment()
    {
        input.RemoveInputProcessor(this);
        panel.Visible = false;
        buildingColor = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Background = EntityPanelFragmentBackground.PurpleStriped,
        };

        panel.AddGameLabel(t.T("LV.CB.Title").Bold()).SetMarginBottom(10);

        for (int i = 0; i < sliders.Length; i++)
        {
            var slider = sliders[i] = panel.AddSliderInt(
                label: SliderTexts[i],
                name: "Color-" + i,
                values: new(0, 255, 255));

            slider.AddEndLabel(i => i.ToString());

            var index = i;
            slider.RegisterChange(v => ChangeColorPart(index, v));
        }

        rotationSlider = panel.AddSliderInt(t.T("LV.CB.Rotation"), values: new(-180, 180, 0));
        rotationSlider
            .RegisterChange(SetRotation)
            .AddEndLabel(v => v + "°");

        panel.AddGameButton(t.T("LV.CB.Reset"), Reset, "ResetColor", stretched: true)
            .SetMargin(top: 20)
            .SetFlexGrow();

        return panel.Initialize(initializer);
    }

    public void ShowFragment(BaseComponent entity)
    {
        buildingColor = entity.GetComponentFast<BuildingColorComponent>();
        if (buildingColor is null)
        {
            panel.Visible = false;
            return;
        }

        UpdatePanelContent();
        input.AddInputProcessor(this);
    }

    public void UpdateFragment() { }

    void ChangeColorPart(int index, int value)
    {
        if (buildingColor is null) { return; }

        var color = buildingColor.Color ?? DefaultColor;
        color[index] = value;

        buildingColor.SetColor(color);
    }

    void SetRotation(int rotation)
    {
        buildingColor?.SetRotation(rotation);
    }

    void Reset()
    {
        buildingColor?.Reset();
        UpdatePanelContent();
    }

    void UpdatePanelContent()
    {
        var color = buildingColor!.Color ?? DefaultColor;
        UpdateColorSliders(color);
        rotationSlider.SetValue(buildingColor.Rotation ?? 0);

        panel.Visible = true;
    }

    public bool ProcessInput()
    {
        if (buildingColor is null) { return false; }

        if (input.IsKeyHeld(CopyColorKeyId))
        {
            if (buildingColor.Color is null) { return false; }

            clipboard = buildingColor.Color;
            return true;
        }
        else if (input.IsKeyHeld(PasteColorKeyId))
        {
            if (clipboard is null) { return false; }

            buildingColor.SetColor(clipboard.Value);
            UpdateColorSliders(clipboard.Value);
            return true;
        }

        return false;
    }

    void UpdateColorSliders(in Vector3Int color)
    {
        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i].Slider.value = color[i];
        }
    }
}