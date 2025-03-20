global using Timberborn.InputSystem;

namespace ColorfulBeavers.Graphical;

public class BeaverColorFragment(VisualElementInitializer initializer, ILoc t, InputService input, DialogBoxShower diagShower, EntityRegistry entities) : IEntityPanelFragment, IInputProcessor
{
    const string CopyColorKeyId = "CopyBeaverColor";
    const string PasteColorKeyId = "PasteBeaverColor";

    Vector3Int? clipboard;

    EntityPanelFragmentElement panel = null!;
    BeaverColorComponent? beaverColor;

    static readonly string[] SliderTexts = ["R", "G", "B"];
    readonly GameSliderInt[] sliders = new GameSliderInt[3];

    static readonly Vector3Int DefaultColor = new(255, 255, 255);

    public void ClearFragment()
    {
        input.RemoveInputProcessor(this);
        panel.Visible = false;
        beaverColor = null;
    }

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Background = EntityPanelFragmentBackground.PurpleStriped,
        };

        panel.AddGameLabel(t.T("LV.CBv.Title").Bold()).SetMarginBottom(10);

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

        panel.AddGameButton(t.T("LV.CBv.Reset"), ResetColor, "ResetColor", stretched: true)
            .SetMargin(top: 20)
            .SetFlexGrow();
        panel.AddGameButton(t.T("LV.CBv.ResetAll"), ResetAll, "ResetColorAll", stretched: true)
            .SetFlexGrow();

        return panel.Initialize(initializer);
    }

    public void ShowFragment(BaseComponent entity)
    {
        beaverColor = entity.GetComponentFast<BeaverColorComponent>();
        if (beaverColor is null)
        {
            panel.Visible = false;
            return;
        }

        var color = beaverColor.Color ?? DefaultColor;
        UpdateColorSliders(color);
        panel.Visible = true;
        input.AddInputProcessor(this);
    }

    public void UpdateFragment() { }

    void ChangeColorPart(int index, int value)
    {
        if (beaverColor is null) { return; }

        var color = beaverColor.Color ?? DefaultColor;
        color[index] = value;

        beaverColor.SetColor(color);
    }

    void ResetColor()
    {
        if (beaverColor is null) { return; }

        beaverColor.ResetColor();
        UpdateColorSliders(beaverColor.Color ?? DefaultColor);
    }

    void ResetAll()
    {
        diagShower.Create()
            .SetMessage(t.T("LV.CBv.ResetAllConfirm"))
            .SetConfirmButton(PerformResetAll)
            .SetDefaultCancelButton()
            .Show();
    }

    void PerformResetAll()
    {
        foreach (var e in entities.Entities)
        {
            var colorComp = e.GetComponentFast<BeaverColorComponent>();
            colorComp?.ResetColor();
        }

        ResetColor();
    }

    public bool ProcessInput()
    {
        if (beaverColor is null) { return false; }

        if (input.IsKeyHeld(CopyColorKeyId))
        {
            if (beaverColor.Color is null) { return false; }

            clipboard = beaverColor.Color;
            return true;
        }
        else if (input.IsKeyHeld(PasteColorKeyId))
        {
            if (clipboard is null) { return false; }

            beaverColor.SetColor(clipboard.Value);
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