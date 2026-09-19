namespace GlobalInventory.UI;

[BindSingleton]
public class GlobalInventoryTopBar(
    TopBarPanel topBarPanel,
    VisualElementLoader veLoader,
    NamedIconProvider icons,
    ITooltipRegistrar tooltips,
    ILoc t,
    EventBus eb,
    GlobalInventoryService inventory
) : IPostLoadableSingleton, IUpdatableSingleton
{
    GlobalExtendableTopBarCounter? counter;

    public void PostLoad()
    {
        var root = veLoader.LoadVisualElement("Game/TopBar/ExtendableTopBarCounter");
        root.Q<Image>("Icon").sprite = icons.Materials;
        tooltips.Register(root.Q<VisualElement>("CounterWrapper"), t.T("LV.GI.GroupName"));
        root.Q<Label>("Count").ToggleDisplayStyle(false);

        var items = root.Q<VisualElement>("CounterItems");
        GlobalExtendableTopBarCounter.ConfigureToggling(root, items);

        List<GlobalTopBarCounterRow> rows = [];
        foreach (var handle in inventory.All)
        {
            rows.Add(CreateRow(handle, items));
        }

        var empty = root.Q<Label>("EmptyCounterPlaceholder");
        empty.text = t.T("LV.GI.Nothing");
        counter = new([.. rows], root, empty);
        topBarPanel._root.Add(root);
        counter.UpdateValues();
    }

    public void UpdateSingleton() => counter?.UpdateValues();

    GlobalTopBarCounterRow CreateRow(GlobalGoodHandle handle, VisualElement parent)
    {
        var row = veLoader.LoadVisualElement("Game/TopBar/TopBarCounterRow");
        var spec = handle.Spec;
        row.Q<Image>("Icon").sprite = spec.IconSmall?.Value ?? spec.Icon?.Asset;

        var count = row.Q<Label>("Count");
        var name = new Label(spec.DisplayName.Value);
        name.AddToClassList("game-text-small");
        count.parent.Insert(count.parent.IndexOf(count), name);

        parent.Add(row);
        return new(
            handle,
            row,
            name,
            count,
            row.Q<VisualElement>("Fill"),
            row.Q<VisualElement>("FillFrame"),
            eb);
    }
}
