namespace GlobalInventory.UI;

class GlobalTopBarCounterRow
{
    readonly GlobalGoodHandle handle;
    readonly VisualElement root;
    readonly Label name;
    readonly Label counter;
    readonly VisualElement fillGauge;
    readonly VisualElement fillFrame;
    string? previousText;

    public GlobalTopBarCounterRow(
        GlobalGoodHandle handle,
        VisualElement root,
        Label name,
        Label counter,
        VisualElement fillGauge,
        VisualElement fillFrame,
        EventBus eb)
    {
        this.handle = handle;
        this.root = root;
        this.name = name;
        this.counter = counter;
        this.fillGauge = fillGauge;
        this.fillFrame = fillFrame;
        name.text = handle.Spec.DisplayName.Value;
        root.RegisterCallback<ClickEvent>(_ => eb.Post(new GlobalGoodClickedEvent(handle)));
    }

    public void UpdateValues() => UpdateVisible();

    public bool UpdateVisible()
    {
        var visible = handle.IsVisible;
        root.ToggleDisplayStyle(visible);
        if (!visible)
        {
            return false;
        }

        name.text = handle.Spec.DisplayName.Value;
        var text = FormatCount();
        if (previousText != text)
        {
            counter.text = text;
            previousText = text;
        }

        var showFill = handle.FillRate is not null;
        fillFrame.ToggleDisplayStyle(showFill);
        if (showFill)
        {
            fillGauge.SetHeightAsPercent(handle.FillRate!.Value);
        }

        return true;
    }

    string FormatCount()
    {
        var amount = GlobalInventoryUi.Format(handle.Amount);
        if (handle.MaxCapacity is float max && max != 0)
        {
            return $"{amount} / {GlobalInventoryUi.Format(max)}";
        }

        return amount;
    }
}
