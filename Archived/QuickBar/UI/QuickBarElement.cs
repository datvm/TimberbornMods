namespace QuickBar.UI;

public class QuickBarElement(
    IAssetLoader assets,
    QuickBarService service,
    QuickBarHotkeyService hotkeys,
    UILayout uiLayout,
    VisualElementLoader veLoader,
    VisualElementInitializer veInit
) : VisualElement, ILoadableSingleton
{
    public const int ItemsPerGroup = 10;

    bool init;
    bool locationVertical;
    bool collapsed;

#nullable disable
    Texture2D bg;
    VisualElement content;
    VisualElement itemContainer;

    VisualTreeAsset buttonVisual;
    Button btnCollapse, btnExpand;
#nullable enable

    VisualElement? headerButtons;
    QuickBarItemElement[] itemElements = [];

    public event Action? OnChangeLocationRequested;

    public void Load()
    {
        pickingMode = PickingMode.Ignore;
        name = "QuickBar";

        bg = assets.Load<Texture2D>("UI/Images/BottomBar/bar-bg");
        buttonVisual = veLoader.LoadVisualTreeAsset("Common/BottomBar/GrouplessToolButton");

        service.ItemChanged += OnItemChanged;
    }

    private void OnItemChanged(int index, IQuickBarItem? item)
    {
        if (index < 0 || index >= itemElements.Length) { return; }

        var element = itemElements[index];
        element.SetItem(
            item,
            hotkeys.GetShortcutText(index),
            buttonVisual.Instantiate().Initialize(veInit));
    }

    public void SwitchLocation(bool vertical)
    {
        if (locationVertical == vertical && init) { return; }
        locationVertical = vertical;
        init = true;

        ReloadUI();
        AttachSelf(vertical);
    }

    void AttachSelf(bool vertical)
    {
        var s = style;
        s.left = 0;
        s.top = 0;

        if (vertical)
        {
            s.position = Position.Relative;
            uiLayout._bottomRight.Add(this);
        }
        else
        {
            s.position = Position.Absolute;
            s.width = new Length(100, LengthUnit.Percent);
            s.justifyContent = Justify.Center;
            s.alignItems = Align.Center;

            uiLayout._panelStack._root.Add(this);
        }
    }

    void ReloadUI()
    {
        Clear();

        content = this.AddChild();
        content.style.backgroundImage = bg;
        content.Add(InitOrGetHeaderButtons());

        AddItems();

        this.Initialize(veInit);
        ToggleCollapse(collapsed);
    }

    void AddItems()
    {
        itemContainer = content.AddChild(name: "QuickBarItems");
        itemContainer.style.flexDirection = locationVertical
            ? FlexDirection.RowReverse
            : FlexDirection.Column;
        itemContainer.style.flexWrap = Wrap.NoWrap;

        VisualElement currGrp = default!;
        var currGrpCount = ItemsPerGroup;

        var items = service.Items;
        var count = items.Count;
        itemElements = new QuickBarItemElement[count];
        for (int i = 0; i < count; i++)
        {
            var item = items[i];

            if (currGrpCount >= ItemsPerGroup)
            {
                currGrp = itemContainer.AddChild(name: "QuickBarItemGroup");

                if (!locationVertical)
                {
                    currGrp.SetAsRow();
                }

                currGrpCount = 0;
            }

            var el = currGrp.AddChild<QuickBarItemElement>()
                .SetItem(
                    item,
                    hotkeys.GetShortcutText(i),
                    buttonVisual.Instantiate());
            itemElements[i] = el;

            currGrpCount++;
        }
    }

    VisualElement InitOrGetHeaderButtons()
    {
        if (headerButtons is null)
        {
            headerButtons = content.AddRow();

            var spacer = headerButtons.AddChild().SetMarginLeftAuto();

            btnExpand = headerButtons.AddGameButton("+", onClick: () => ToggleCollapse(false)).SetPadding(10, 5);
            btnCollapse = headerButtons.AddGameButton("-", onClick: () => ToggleCollapse(true)).SetPadding(10, 5);
            headerButtons.AddGameButton("⇋", onClick: () => OnChangeLocationRequested?.Invoke()).SetPadding(10, 5);
        }

        return headerButtons;
    }

    void ToggleCollapse(bool collapsed)
    {
        this.collapsed = collapsed;
        itemContainer.SetDisplay(!collapsed);
        btnCollapse.SetDisplay(!collapsed);
        btnExpand.SetDisplay(collapsed);
    }

}
