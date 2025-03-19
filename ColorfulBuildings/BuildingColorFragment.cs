namespace ColorfulBuildings;

public class BuildingColorFragment(VisualElementInitializer initializer, ILoc t) : IEntityPanelFragment
{
    EntityPanelFragmentElement panel = null!;
    BuildingColorComponent? buildingColor;

    static readonly string[] SliderTexts = ["R", "G", "B"];
    readonly GameSliderInt[] sliders = new GameSliderInt[3];

    static readonly Vector3Int DefaultColor = new(255, 255, 255);

    public void ClearFragment()
    {
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

        panel.AddGameButton(t.T("LV.CB.Reset"), ResetColor, "ResetColor", stretched: true)
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

        var color = buildingColor.Color ?? DefaultColor;
        for (int i = 0; i < sliders.Length; i++)
        {
            sliders[i].Slider.value = color[i];
        }

        panel.Visible = true;
    }

    public void UpdateFragment() { }

    void ChangeColorPart(int index, int value)
    {
        if (buildingColor is null) { return; }

        var color = buildingColor.Color ?? DefaultColor;
        color[index] = value;

        buildingColor.SetColor(color);
    }

    void ResetColor()
    {
        buildingColor?.ClearColor();
    }

}