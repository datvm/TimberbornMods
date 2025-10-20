namespace TimberUi.CommonUi;

public class DialogBoxElement : VisualElement
{
    const string DialogClass = "content-row-centered";
    static readonly ImmutableArray<string> BoxClasses = ["options-panel"];
    static readonly ImmutableArray<string> ContainerClasses = ["sliced-border", "box__content-container"];

    public Label? TitleEl { get; private set; }
    public string? Title { get; private set; }

    public Button? CloseButton { get; private set; } = null!;
    bool hasCustomCloseAction;
    bool closeButtonEventSet;
    Action? pendingSizeChange;

    public VisualElement Container { get; private set; }
    public ScrollView Content { get; private set; }
    public DialogBox? DialogBox { get; private set; }

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

        CloseButton ??= Container.AddCloseButton("CloseButton");

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

    public DialogBoxElement SetDialogPercentSize(float? width = default, float? height = default)
        => SetDialogSize(
            width: width is null ? null : Mathf.FloorToInt(Screen.width * width.Value),
            height: height is null ? null : Mathf.FloorToInt(Screen.height * height.Value),
            scaled: true
        );

    public DialogBoxElement SetDialogSize(int? width = default, int? height = default, bool scaled = true)
    {
        void PerformChangeSize()
        {
            var scale = (scaled && panel is not null) ? panel.scaledPixelsPerPoint : 1;

            if (width is not null)
            {
                var w = width.Value / scale;
                
                this.Q("Box").SetWidth(w);
                this.SetMinMaxSize(w, null);
            }

            if (height is not null)
            {
                var h = height.Value / scale;
                Content.style.maxHeight = new StyleLength(StyleKeyword.Auto);
                Content.style.height = h;
            }
        }

        if (scaled && panel is null)
        {
            pendingSizeChange = PerformChangeSize;
        }
        else
        {
            pendingSizeChange = null;
            PerformChangeSize();
        }

        return this;
    }

    public DialogBox Show(VisualElementLoader loader, PanelStack panelStack, Action? confirm = default, Action? cancel = default)
        => Show(loader._visualElementInitializer, panelStack, confirm, cancel);

    public DialogBox Show(VisualElementInitializer? initializer, PanelStack panelStack, Action? confirm = default, Action? cancel = default)
    {
        if (initializer is not null)
        {
            this.Initialize(initializer);
        }

        var diag = DialogBox = new(
            panelStack,
            confirm ?? TimberUiUtils.DoNothing,
            cancel ?? TimberUiUtils.DoNothing,
            this
        );

        if (!closeButtonEventSet)
        {
            closeButtonEventSet = true;

            if (CloseButton is not null && !hasCustomCloseAction)
            {
                CloseButton.clicked += OnUICancelled;
            }
        }

        panelStack.PushDialog(diag);

        if (pendingSizeChange is not null)
        {
            pendingSizeChange();
            pendingSizeChange = null;
        }

        return diag;
    }

    public async Task<bool> ShowAsync(VisualElementInitializer? initializer, PanelStack panelStack, Action? confirm = default, Action? cancel = default)
    {
        TaskCompletionSource<bool> tcs = new();

        Show(initializer, panelStack,
            confirm: () =>
            {
                tcs.SetResult(true);
                confirm?.Invoke();
            },
            cancel: () =>
            {
                tcs.SetResult(false);
                cancel?.Invoke();
            });

        return await tcs.Task;
    }


    public void OnUICancelled()
    {
        DialogBox?.OnUICancelled();
    }

    public void OnUIConfirmed()
    {
        DialogBox?.OnUIConfirmed();
    }

    public void Close()
    {
        DialogBox?.Close();
    }

}