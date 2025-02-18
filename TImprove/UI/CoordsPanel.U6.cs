namespace TImprove.UI;

#if TIMBER6
public class CoordsPanel(UILayout layout, UIBuilder builder) : ILoadableSingleton
{
    NineSliceVisualElement? panel;
    Label? lblCoords;

    public void Load()
    {
        const string LabelName = "Coords";

        panel = builder.Create<VisualElementBuilder>().SetName("HeightShowerContainer").AddClass("top-right-item")
            .AddClass("square-large--green")
            .SetFlexDirection(FlexDirection.Row)
            .SetFlexWrap(Wrap.Wrap)
            .SetJustifyContent(Justify.Center)
            .AddComponent<LabelBuilder>(
                LabelName,
                (builder) => builder
                    .AddClass("text--centered")
                    .AddClass("text--yellow")
                    .AddClass("date-panel__text")
                    .AddClass("game-text--normal"))
            .BuildAndInitialize();

        lblCoords = panel.Q<Label>(LabelName);
        layout.AddTopRight(panel, 7);

        SetVisibility(false);
    }

    public void SetVisibility(bool visible)
    {
        if (panel is null) { return; }

        panel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SetText(string text)
    {
        if (lblCoords is null) { return; }

        lblCoords.text = text;
    }

}
#endif