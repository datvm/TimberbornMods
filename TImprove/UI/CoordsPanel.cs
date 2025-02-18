namespace TImprove.UI;

#if TIMBER7
public class CoordsPanel(UILayout layout) : ILoadableSingleton
{
    NineSliceVisualElement panel = null!;
    Label lblCoords = null!;

    public void Load()
    {
        panel = new NineSliceVisualElement();
        panel.classList.AddRange(["top-right-item", "square-large--green"]);
        panel.style.flexDirection = FlexDirection.Row;
        panel.style.justifyContent = Justify.Center;

        lblCoords = new Label();
        panel.Add(lblCoords);
        lblCoords.classList.AddRange(["text--centered", "text--yellow", "game-text--normal"]);

        layout.AddTopRight(panel, 7);

        SetVisibility(false);
    }

    public void SetVisibility(bool visible)
    {
        panel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SetText(string text)
    {
        lblCoords.text = text;
    }

}
#endif