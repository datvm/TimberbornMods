namespace ConfigurableToolGroups.UI;

public abstract class SimpleRootButton : CustomBottomBarElement
{
    readonly VisualElementLoader loader;
    readonly ToolButtonFactory toolButtonFactory;

    public event Action Clicked;

    protected abstract void OnClicked();
    
    protected RootToolButtonColor Color { get; set; } = RootToolButtonColor.Red;
    protected abstract string ImageName { get; }
    protected string? Text { get; set; }

    protected VisualElement? Button { get; set; }

    public SimpleRootButton(
        VisualElementLoader loader,
        ToolButtonFactory toolButtonFactory
    )
    {
        this.loader = loader;
        this.toolButtonFactory = toolButtonFactory;

        Clicked += OnClicked;
    }

    public override IEnumerable<BottomBarElement> GetElements()
    {
        var btn = Button = loader.LoadVisualElement("Common/BottomBar/GrouplessToolButton");
        btn.AddToClassList("bottom-bar-button--" + Color.ToString().ToLowerInvariant());

        var sprite = toolButtonFactory.LoadImage(ImageName);
        btn.Q<VisualElement>("ToolImage").style.backgroundImage = new(sprite);

        if (!string.IsNullOrEmpty(Text))
        {
            btn.Q<TextElement>("BottomText").text = Text;
        }

        btn.RegisterCallback<ClickEvent>(e => Clicked());

        yield return BottomBarElement.CreateSingleLevel(btn);
    }

}
