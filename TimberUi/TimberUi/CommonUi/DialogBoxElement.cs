namespace UiBuilder.CommonUi;

public class DialogBoxElement : VisualElement
{
    static readonly ImmutableArray<string> MainClasses = ["content-row-centered"];
    static readonly ImmutableArray<string> NineSliceClasses = ["content-centered", "sliced-border", "sliced-border--nontransparent"];
    static readonly ImmutableArray<string> BoxClasses = ["box"];

    public VisualElement Container { get; private set; }

    public Label? TitleEl { get; private set; }
    public string? Title { get; private set; }

    public Button? CloseButton { get; private set; }
    bool hasCustomCloseAction;

    public VisualElement Content { get; private set; }

    public DialogBoxElement() : this(false) { }

    public DialogBoxElement(bool scrollAsBox)
    {
        classList.AddRange(MainClasses);

        Container = this.AddChild<NineSliceVisualElement>(name: "Container", classes: NineSliceClasses);

        var type = scrollAsBox ? typeof(ScrollView) : typeof(VisualElement);
        Content = Container.AddChild(type, name: "Box", BoxClasses);
    }

    public DialogBoxElement AddCloseButton(Action? customAction = default)
    {
        if (CloseButton is not null)
        {
            throw new InvalidOperationException("Close button already exists.");
        }

        CloseButton = new Button();
        CloseButton.classList.Add("close-button");

        Container.Add(CloseButton);

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
            var wrapper = this.Q("Container")
                .AddChild<NineSliceVisualElement>(classes: ["capsule-header", "capsule-header--lower", "content-centered"]);

            TitleEl = wrapper.AddChild<Label>(title, ["capsule-header__text"]);
        }

        TitleEl.text = Title = title;

        return this;
    }

    public DialogBox Show(PanelStack panelStack, Action? confirm = default, Action? cancel = default)
    {
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
