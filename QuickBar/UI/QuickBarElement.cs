namespace QuickBar.UI;

public class QuickBarElement(
    IAssetLoader assets,
    QuickBarService service,
    UILayout uiLayout,
    VisualElementLoader veLoader,
    VisualElementInitializer veInit
) : VisualElement, ILoadableSingleton
{
    public const int GroupItemCount = 10;

    bool init;
    bool locationVertical;

#nullable disable
    VisualElement content;
    VisualElement itemContainer;

    VisualTreeAsset buttonVisual;
#nullable enable

    QuickBarItemElement[] itemElements = [];

    Button? changeLocation;
    public event Action? OnChangeLocationRequested;

    public void Load()
    {
        name = "QuickBar";

        var bg = assets.Load<Texture2D>("UI/Images/BottomBar/bar-bg");
        style.backgroundImage = bg;

        buttonVisual = veLoader.LoadVisualTreeAsset("Common/BottomBar/GrouplessToolButton");

        service.ItemChanged += OnItemChanged;
    }

    private void OnItemChanged(int index, IQuickBarItem? item)
    {
        if (index < 0 || index >= itemElements.Length) { return; }

        var element = itemElements[index];
        element.SetItem(
            item, 
            service.GetShortcutText(index),
            buttonVisual.Instantiate().Initialize(veInit));
    }

    public void SwitchLocation(bool vertical)
    {
        if (locationVertical == vertical && init) { return; }
        locationVertical = vertical;
        init = true;

        Debug.Log($"QuickBar location changed to {(vertical ? "vertical" : "horizontal")}.");

        ReloadUI();

        if (vertical)
        {
            uiLayout._bottomRight.Add(this);
        }
        else
        {
            uiLayout._topBar.Add(this);
        }
    }

    void ReloadUI()
    {
        Clear();

        content = this.AddChild();
        if (!locationVertical)
        {
            content.SetAsRow();
        }

        content.Add(InitOrGetChangeLocationButton());

        AddItems();

        this.Initialize(veInit);
    }

    void AddItems()
    {
        itemContainer = content.AddChild();

        var itemsPerRow = CalculateItemPerRow();
        var currRow = itemContainer.AddRow();
        var currGrp = 0;

        var items = service.Items;
        var count = items.Count;
        itemElements = new QuickBarItemElement[count];
        for (int i = 0; i < count; i++)
        {
            var item = items[i];

            if (currGrp >= itemsPerRow)
            {
                currRow = itemContainer.AddRow();
                currGrp = 0;
            }

            var el = currRow.AddChild<QuickBarItemElement>()
                .SetItem(
                    item,
                    service.GetShortcutText(i),
                    buttonVisual.Instantiate());
            itemElements[i] = el;

            currGrp++;
        }
    }

    int CalculateItemPerRow()
    {
        var count = service.Items.Count;

        return locationVertical // Using ceiling division
            ? (count + GroupItemCount - 1) / GroupItemCount
            : GroupItemCount;
    }

    Button InitOrGetChangeLocationButton()
    {
        if (changeLocation is not null) { return changeLocation; }

        var row = content.AddRow();

        changeLocation = row.AddGameButton("⇋", onClick: () => OnChangeLocationRequested?.Invoke())
            .SetMarginLeftAuto()
            .SetPadding(5);

        return changeLocation;
    }

}
