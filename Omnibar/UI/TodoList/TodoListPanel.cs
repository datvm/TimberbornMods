namespace Omnibar.UI.TodoList;

public class TodoListPanel(
    ILoc t,
    VisualElementInitializer veInit,
    ToDoListManager man,
    IAssetLoader assetLoader,
    ScienceService scienceService,
    GoodItemFactory goodItemFactory,
#pragma warning disable CS9113 // This is here to make sure the PostLoad is run after this service
    ToolUnlockingService _toolUnlockingService
#pragma warning restore CS9113 // Parameter is unread.
) : ILoadableSingleton, ITickableSingleton, IPostLoadableSingleton
{

#nullable disable
    public NineSliceVisualElement Root { get; private set; }
    VisualElement content;
    Button btnExpand, btnCollapse;
    Label lblNoItem;
    ScrollView itemsContainer;

    public event Action<int?> OnToDoListRequested;
    Texture2D timerIcon;
#nullable enable

    bool expanded = true;
    readonly List<TodoListPanelItem> items = [];

    public void Load()
    {
        timerIcon = assetLoader.Load<Texture2D>("Sprites/Omnibar/timer");

        Root = InitRoot();

        AddHeader(Root);
        content = AddContent(Root);

        SetExpansionDisplay();
        Root.Initialize(veInit);

        man.EntriesChanged += ReloadList;
    }

    public void PostLoad()
    {
        // Only now that the tool lockers are in place
        ReloadList(null);
    }

    VisualElement AddHeader(VisualElement parent)
    {
        var header = parent.AddRow().SetFlexShrink(0);

        header.AddGameLabel("LV.OB.TodoList".T(t).Bold());

        var space = header.AddChild().SetMarginLeftAuto();

        btnExpand = header.AddPlusButton("ExpandTodoList", UiBuilder.GameButtonSize.Small)
            .AddAction(ToggleExpansion);
        btnCollapse = header.AddMinusButton("CollapseTodoList", UiBuilder.GameButtonSize.Small)
            .AddAction(ToggleExpansion);

        return header;
    }

    VisualElement AddContent(VisualElement parent)
    {
        var el = parent.AddChild()
            .SetFlexGrow();

        el.AddGameButton("LV.OB.ManageTodo".T(t), onClick: () => OnToDoListRequested(null))
            .SetFlexShrink(0)
            .SetMargin(top: 10, bottom: 10);

        itemsContainer = el.AddScrollView()
            .SetFlexGrow();

        lblNoItem = el.AddGameLabel("LV.OB.TodoListEmpty".T(t))
            .RemoveSelf();

        return el;
    }

    void ToggleExpansion()
    {
        expanded = !expanded;
        SetExpansionDisplay();
    }

    void SetExpansionDisplay()
    {
        content.SetDisplay(expanded);
        btnExpand.SetDisplay(!expanded);
        btnCollapse.SetDisplay(expanded);
    }

    public void ReloadList(List<ToDoListEntry>? entries)
    {
        itemsContainer.Clear();
        items.Clear();

        entries = [.. (entries ?? man.Entries).Where(q => q.Pin)];

        if (entries.Count == 0)
        {
            itemsContainer.Add(lblNoItem);
        }
        else
        {
            foreach (var entry in entries)
            {
                var item = new TodoListPanelItem(entry, timerIcon, scienceService, goodItemFactory);
                items.Add(item);
                itemsContainer.Add(item);
            }
        }
    }

    static NineSliceVisualElement InitRoot()
    {
        return new NineSliceVisualElement()
            .AddClass("square-large--green")
            .SetPadding(10)
            .SetSize(width: 260)
            .SetMaxHeight(500);
    }

    public void Tick()
    {
        foreach (var item in items)
        {
            item.UpdateData();
        }
    }

    
}
