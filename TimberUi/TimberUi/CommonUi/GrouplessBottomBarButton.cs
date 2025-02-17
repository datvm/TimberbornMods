namespace TimberUi.CommonUi;

public class GrouplessBottomBarButton(VisualElementLoader veLoader, IAssetLoader assetLoader) : IBottomBarElementProvider
{
    public List<string> Classes { get; set; } = [UiCssClasses.BottomBarButtonRed];

    public Sprite? Sprite { get; set; }
    public string SpritePath { get; set; } = "Sprites/BottomBar/Options";

    public EventCallback<ClickEvent>? ClickCallback { get; set; }
    public Action Click { set => ClickCallback = new EventCallback<ClickEvent>(_ => value()); }

    public VisualElement? MultiLevelElement { get; set; }

    public string? BottomText { get; set; }

    public VisualElement? Button { get; set; }

    public virtual VisualElement Build()
    {
        if (Button is not null)
        {
            return Button;
        }

        Sprite ??= assetLoader.Load<Sprite>(SpritePath);

        var ve = veLoader.LoadVisualElement("Common/BottomBar/GrouplessToolButton");
        ve.classList.AddRange(Classes);
        ve.Q("ToolImage").style.backgroundImage = new StyleBackground(Sprite);

        var btn = ve.Q<Button>("ToolButton");
        if (ClickCallback is not null)
        {
            btn.RegisterCallback(ClickCallback);
        }

        if (BottomText is not null)
        {
            ve.Q<TextElement>("BottomText").text = BottomText;
        }

        return Button = ve;
    }

    public BottomBarElement GetElement() => MultiLevelElement is null 
        ? BottomBarElement.CreateSingleLevel(Build())
        : BottomBarElement.CreateMultiLevel(Build(), MultiLevelElement);

}
