namespace GlobalInventory.UI;

class GlobalTopBarCounterRow
{
    readonly GlobalGoodHandle handle;
    readonly VisualElement root;
    readonly Label counter;
    readonly VisualElement fillGauge;
    readonly VisualElement fillFrame;
    string? previousText;

    public GlobalTopBarCounterRow(
        GlobalGoodHandle handle,
        VisualElement root,
        Label counter,
        VisualElement fillGauge,
        VisualElement fillFrame,
        EventBus eb)
    {
        this.handle = handle;
        this.root = root;
        this.counter = counter;
        this.fillGauge = fillGauge;
        this.fillFrame = fillFrame;
        root.RegisterCallback<ClickEvent>(_ => eb.Post(new GlobalGoodClickedEvent(handle)));
    }

    public void Update(out bool isVisible)
    {
        isVisible = handle.IsVisible;
        root.ToggleDisplayStyle(isVisible);
        if (!isVisible)
        {
            return;
        }

        var text = GlobalInventoryUi.Format(handle.Amount);
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
    }
}
