namespace Omnibar.UI;

public class OmnibarBox : IPanelController, ILoadableSingleton
{

    readonly VisualElement box;
    Texture2D question = null!;

    public bool IsOpen { get; private set; }
    readonly NineSliceTextField txtContent;
    readonly ListView lstItems;

    readonly PanelStack panelStack;
    readonly OmnibarService omnibarService;
    readonly IAssetLoader assetLoader;
    readonly VisualElementInitializer veInit;

    List<OmnibarFilteredItem>? items;
    int selectingIndex = -1;

    public OmnibarBox(PanelStack panelStack, OmnibarService omnibarService, VisualElementInitializer veInit, IAssetLoader assetLoader)
    {
        this.panelStack = panelStack;
        this.omnibarService = omnibarService;
        this.assetLoader = assetLoader;
        this.veInit = veInit;

        box = CreateBox();
        var container = CreateBoxContainer(box);

        txtContent = CreateContentText(container);
        txtContent.RegisterValueChangedCallback(_ => OnTextChanged());

        lstItems = CreateListView(container);
        lstItems.makeItem = MakeListItem;
        lstItems.bindItem = BindListItem;
    }

    public void Load()
    {
        question = assetLoader.Load<Texture2D>("Sprites/Omnibar/question");
        veInit.InitializeVisualElement(box);
    }

    public bool ShouldOpen()
    {
        if (IsOpen) { return false; }

        Open();
        return true;
    }

    public void Open()
    {
        txtContent.text = "";
        panelStack.PushDialog(this);

        IsOpen = true;
        OnTextChanged();
        txtContent.Focus();
    }

    public VisualElement GetPanel() => box;

    public bool OnUIConfirmed()
    {
        Close();
        return true;
    }

    public void OnUICancelled()
    {
        Close();
    }

    public void Close()
    {
        IsOpen = false;
        panelStack.Pop(this);
    }

    void OnTextChanged()
    {
        items = null;
        var kw = txtContent.text?.Trim();
        if (string.IsNullOrEmpty(kw))
        {
            lstItems.visible = false;
            return;
        }

        items = omnibarService.GetItems(kw.ToLower());
        if (items.Count == 0)
        {
            lstItems.visible = false;
            return;
        }

        lstItems.itemsSource = items;
        SetSelectingIndex(0);
        lstItems.visible = true;
    }

    OmnibarListItem MakeListItem() => new(question);

    void BindListItem(VisualElement ve, int index)
    {
        if (items is null) { return; } // Should not happen

        var item = items[index];
        var el = (OmnibarListItem)ve;

        el.SetItem(item);
    }

    void OnItemSelected(OmnibarListItem item)
    {

    }

    void SetSelectingIndex(int index)
    {
        if (items is null) { return; }

        if (selectingIndex > -1 && selectingIndex < items.Count)
        {
            
        }
    }

    static VisualElement CreateBox()
    {
        VisualElement box = new();
        var boxS = box.style;
        boxS.minWidth = boxS.maxWidth = new Length(100, LengthUnit.Percent);
        boxS.minHeight = boxS.maxHeight = new Length(100, LengthUnit.Percent);
        boxS.alignItems = Align.Center;
        boxS.justifyContent = Justify.Center;

        return box;
    }

    static VisualElement CreateBoxContainer(VisualElement box)
    {
        var container = box.AddChild<NineSliceVisualElement>(classes: [UiCssClasses.ButtonTopBarPrefix + UiCssClasses.Green])
            .SetPadding(20);

        container.style.width = new Length(70, LengthUnit.Percent);

        return container;
    }

    static NineSliceTextField CreateContentText(VisualElement container)
    {
        var txt = container.AddTextField("OmnibarTextField");
        txt.style.width = new Length(100, LengthUnit.Percent);
        txt.style.fontSize = 22;

        return txt;
    }

    static ListView CreateListView(VisualElement container)
    {
        var lst = container.AddListView("OmnibarListView")
            .SetMargin(top: 30)
            .SetSize(height: 150)
            .SetFlexGrow(1);

        lst.fixedItemHeight = OmnibarListItem.ItemHeight;

        return lst;
    }

}
