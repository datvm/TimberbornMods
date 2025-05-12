namespace Omnibar.UI;

public class OmnibarBox : IPanelController, ILoadableSingleton
{
    readonly VisualElement box;

    public bool IsOpen { get; private set; }
    readonly NineSliceTextField txtContent;
    readonly OmnibarBoxListView lstItems;

    readonly PanelStack panelStack;
    readonly OmnibarService omnibarService;
    readonly IAssetLoader assetLoader;
    readonly VisualElementInitializer veInit;

    List<OmnibarFilteredItem>? items;

    public OmnibarBox(
        PanelStack panelStack,
        OmnibarService omnibarService,
        VisualElementInitializer veInit,
        IAssetLoader assetLoader
    )
    {
        this.panelStack = panelStack;
        this.omnibarService = omnibarService;
        this.assetLoader = assetLoader;
        this.veInit = veInit;

        box = CreateBox();
        var container = CreateBoxContainer(box);

        txtContent = CreateContentText(container);
        txtContent.RegisterValueChangedCallback(_ => OnTextChanged());

        box.RegisterCallback<KeyUpEvent>(ProcessTextInput);

        lstItems = container.AddChild<OmnibarBoxListView>();
    }

    public void Load()
    {
        lstItems.QuestionTexture = assetLoader.Load<Texture2D>("Sprites/Omnibar/question");
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
        var selectingItem = lstItems.SelectingItem;

        IInplaceExecutionOmnibarItem? inplace = selectingItem?.Item as IInplaceExecutionOmnibarItem;
        if (inplace is null)
        {
            Close();
        }
        if (selectingItem is null) { return false; }

        if (inplace is null)
        {
            selectingItem.Value.Item.Execute();
        }
        else
        {
            inplace.Execute(this);
        }

        return true;
    }

    public void AddToTodoList(IOmnibarItemWithTodoList item)
    {
        item.AddToTodoList();
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

    public void SetText(string text)
    {
        txtContent.text = text;
        txtContent.selectAllOnFocus = false;
        txtContent.Focus();

        var pos = text.Length;
        txtContent.textSelection.SelectRange(pos, pos);

        OnTextChanged();
    }

    void OnTextChanged()
    {
        items = null;
        var kw = txtContent.text?.TrimStart() ?? "";

        items = omnibarService.GetItems(kw!.ToLower());
        lstItems.SetItems(items);
    }

    void ProcessTextInput(KeyUpEvent e)
    {
        var processed = false;

        if (e.keyCode == KeyCode.UpArrow)
        {
            processed = lstItems.SelectItemWithKey(-1);
        }
        else if (e.keyCode == KeyCode.DownArrow)
        {
            processed = lstItems.SelectItemWithKey(1);
        }
        else if (e.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
        {
            processed = true;
            OnUIConfirmed();
        }
        else if (e.keyCode is KeyCode.Escape)
        {
            processed = true;
            OnUICancelled();
        }
        else if (e.keyCode == TodoListController.AddBuildingToTodoListKeyCode)
        {
            var item = lstItems.SelectingItem;
            if (item?.Item is IOmnibarItemWithTodoList tdItem && tdItem.CanAddToTodoList)
            {
                processed = true;
                AddToTodoList(tdItem);
            }
        }

        if (processed)
        {
            e.StopImmediatePropagation();
            e.StopPropagation();
            txtContent.focusController?.IgnoreEvent(e);
        }
    }

    static VisualElement CreateBox()
    {
        var box = new VisualElement()
            .SetMinMaxSizePercent(100, 100)
            .AlignItems()
            .JustifyContent();

        return box;
    }

    static VisualElement CreateBoxContainer(VisualElement box)
    {
        var container = box.AddChild<NineSliceVisualElement>(classes: [UiCssClasses.ButtonTopBarPrefix + UiCssClasses.Green])
            .SetPadding(20)
            .SetWidthPercent(70);

        return container;
    }

    static NineSliceTextField CreateContentText(VisualElement container)
    {
        var txt = container.AddTextField("OmnibarTextField")
            .SetWidthPercent(100);
        txt.style.fontSize = 22;

        return txt;
    }

}
