namespace UiBuilder.CommonUi;

public class DialogBoxElement : VisualElement
{
    const string DialogClass = "content-row-centered";
    static readonly ImmutableArray<string> BoxClasses = ["options-panel"];
    static readonly ImmutableArray<string> ContainerClasses = ["sliced-border", "box__content-container"];

    public Label? TitleEl { get; private set; }
    public string? Title { get; private set; }

    public Button? CloseButton { get; private set; } = null!;
    bool hasCustomCloseAction;

    public VisualElement Container { get; private set; }
    public ScrollView Content { get; private set; }

    public DialogBoxElement()
    {
        this.AddClass(DialogClass);

        var box = this.AddChild(name: "Box", classes: BoxClasses);

        Container = box.AddChild<NineSliceVisualElement>(name: "Container", classes: ContainerClasses);
        Content = Container.AddScrollView(name: "Content");
    }

    static readonly ImmutableArray<string> TitleWrapperClasses = ["capsule-header", "capsule-header--lower", "content-centered",];
    static readonly ImmutableArray<string> TitleClasses = ["capsule-header__text",];
    void AddTitle()
    {
        var wrapper = Container.AddChild<NineSliceVisualElement>(name: "HeaderWrapper", classes: TitleWrapperClasses);
        TitleEl = wrapper.AddLabel(name: "Title", additionalClasses: TitleClasses);
    }

    public DialogBoxElement AddCloseButton(Action? customAction = default)
    {
        if (hasCustomCloseAction)
        {
            throw new InvalidOperationException("Close button already register a custom action.");
        }

        if (CloseButton is null)
        {
            CloseButton = Container.AddCloseButton("CloseButton");
        }

        if (customAction is not null)
        {
            CloseButton.clicked += customAction;
            hasCustomCloseAction = true;
        }

        return this;
    }

    public DialogBoxElement SetTitle(string title)
    {
        if (TitleEl is null)
        {
            AddTitle();
        }

        TitleEl!.text = Title = title;
        return this;
    }

    public DialogBox Show(VisualElementLoader loader, PanelStack panelStack, Action? confirm = default, Action? cancel = default)
    {
        return Show(loader._visualElementInitializer, panelStack, confirm, cancel);
    }

    public DialogBox Show(VisualElementInitializer? initializer, PanelStack panelStack, Action? confirm = default, Action? cancel = default)
    {
        if (initializer is not null)
        {
            this.Initialize(initializer);
        }

        DialogBox diag = new(panelStack, confirm ?? DoNothing, cancel ?? DoNothing, this);

        if (CloseButton is not null && !hasCustomCloseAction)
        {
            CloseButton.clicked += diag.OnUICancelled;
        }

        panelStack.PushDialog(diag);

        return diag;
    }

    static void DoNothing() { }

}
