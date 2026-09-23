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
        root.Q<Image>("Icon").sprite = icons.GetOrLoadTopbar("TopBarGlobalInventory", "GlobalInventory");
        var wrapper = root.Q<VisualElement>("CounterWrapper");
        var count = wrapper.Q<Label>("Count");
        tooltips.Register(wrapper, t.T("LV.GI.GroupName"));
        count.text = t.T("LV.GI.GroupName");

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
        row.Q<Image>("Icon").sprite = handle.Spec.IconSmall?.Value ?? handle.Spec.Icon?.Asset;
        tooltips.Register(row, () => GlobalInventoryUi.Tooltip(handle, t));
        parent.Add(row);
        return new(
            handle,
            row,
            row.Q<Label>("Count"),
            row.Q<VisualElement>("Fill"),
            row.Q<VisualElement>("FillFrame"),
            eb);
    }
}
