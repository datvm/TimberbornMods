namespace MoreOverlay.Services.Providers;

[MultiBind(typeof(IMoreOverlayProvider))]
public class InventoryOverlayProvider(NamedIconProvider namedIconProvider, IGoodService goodService) : IMoreOverlayProvider
{

    public int Order => 100;

    public bool TrySupporting(MoreOverlayComponent comp, [NotNullWhen(true)] out IMoreOverlayInstance? instance)
    {
        var inv = InventoryFinder.LookForInventories(comp);

        if (inv is null)
        {
            instance = null;
            return false;
        }

        instance = new InventoryOverlayInstance(inv.Value.Inventory, namedIconProvider, goodService);
        return true;
    }

}

public class InventoryOverlayInstance(Inventory inventory, NamedIconProvider namedIconProvider, IGoodService goodService) : IMoreOverlayInstance
{

#nullable disable
    VisualElement inputRow, outputRow;
    readonly Dictionary<string, GoodUIInfo> iconsByGoodId = [];
#nullable enable

    public void Initialize(VisualElement container)
    {
        inputRow = CreateRow("input");
        outputRow = CreateRow("output");

        VisualElement CreateRow(string iconName)
        {
            var row = container.AddRow(iconName).AlignItems().SetMarginBottom(5).SetMaxWidth(150).SetWrap();
            row.AddImage(namedIconProvider.GetOrLoadGameIcon(iconName, iconName)).SetSize(MoreOverlayUtils.IconSize);

            return row;
        }
    }

    public void OnShow()
    {
        inventory.InventoryChanged += OnInventoryChanged;
        UpdateData();
    }

    public void OnHide()
    {
        inventory.InventoryChanged -= OnInventoryChanged;
    }

    void OnInventoryChanged(object sender, InventoryChangedEventArgs e) => UpdateData();

    void UpdateData()
    {
        HashSet<string> exists = [];
        bool hasInput = false, hasOutput = false;

        foreach (var g in inventory.Stock)
        {
            var (icon, isInput, limit) = GetOrCreate(g.GoodId);
            var text = limit > 0 ? $"{g.Amount}/{limit}" : g.Amount.ToString();
            icon.SetDisplay(true).SetPostfixText(text);

            if (isInput)
            {
                hasInput = true;
            }
            else
            {
                hasOutput = true;
            }
            exists.Add(g.GoodId);
        }

        inputRow.SetDisplay(hasInput);
        outputRow.SetDisplay(hasOutput);

        foreach (var (id, el) in iconsByGoodId)
        {
            if (!exists.Contains(id))
            {
                el.IconSpan.SetDisplay(false);
            }
        }
    }

    GoodUIInfo GetOrCreate(string goodId)
    {
        if (!iconsByGoodId.TryGetValue(goodId, out var info))
        {
            var isInput = inventory.InputGoods.Contains(goodId);

            var parent = isInput ? inputRow : outputRow;
            var icon = parent.AddIconSpan().SetGood(goodService, goodId, size: MoreOverlayUtils.IconSize).SetMarginRight(5);

            var capacity = inventory.Capacity == int.MaxValue ? 0 : inventory.GoodCapacity(goodId);
            iconsByGoodId[goodId] = info = new(icon, isInput, capacity);
        }

        return info;
    }

    public void Remove() { }

    readonly record struct GoodUIInfo(IconSpan IconSpan, bool Input, int Limit);
}
