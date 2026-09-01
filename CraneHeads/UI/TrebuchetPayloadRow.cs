namespace CraneHeads.UI;

[BindTransient]
public class TrebuchetPayloadRow(
    IGoodService goods,
    VisualElementInitializer veInit
) : VisualElement
{
    IconSpan icon = null!;
    NineSliceIntegerField field = null!;
    CraneHeadTrebuchetInventory? inventory;
    string goodId = "";
    Action? onListChanged;
    Action? onAmountChanged;
    bool updating;

    public TrebuchetPayloadRow Init(Action onListChanged, Action onAmountChanged)
    {
        this.onListChanged = onListChanged;
        this.onAmountChanged = onAmountChanged;
        this.SetAsRow().AlignItems().SetMarginBottom(5);
        icon = this.AddIconSpan().SetFlexGrow(0).SetFlexShrink(0);
        this.AddChild().SetFlexGrow();
        field = this.AddIntField(changeCallback: value => SetAmount(Math.Clamp(value, 1, 99), rebuild: false))
            .SetWidth(56)
            .SetMargin(left: 3)
            .Initialize(veInit);
        this.AddPlusButton(size: UiBuilder.GameButtonSize.Small)
            .AddAction(() => Change(1))
            .SetMargin(left: 3);
        this.AddMinusButton(size: UiBuilder.GameButtonSize.Small)
            .AddAction(() => Change(-1))
            .SetMargin(left: 3);
        this.AddGameButtonPadded("×", () => SetAmount(0), paddingY: 3).SetMargin(left: 8);
        return this;
    }

    public void Bind(CraneHeadTrebuchetInventory next, string id, int amount)
    {
        inventory = next;
        if (goodId != id)
        {
            goodId = id;
            icon.SetGood(goods, id, showName: true);
        }

        updating = true;
        field.SetValueWithoutNotify(amount);
        updating = false;
    }

    void Change(int delta)
    {
        if (inventory is null)
        {
            return;
        }

        SetAmount(Math.Clamp(inventory.Requested.GetValueOrDefault(goodId) + delta, 1, 99), rebuild: false);
    }

    void SetAmount(int value, bool rebuild = true)
    {
        if (updating || inventory is null)
        {
            return;
        }

        if (value > 0)
        {
            value = Math.Clamp(value, 1, 99);
        }

        inventory.TrySetGood(goodId, value);
        updating = true;
        if (value > 0)
        {
            field.SetValueWithoutNotify(value);
        }

        updating = false;
        if (rebuild || value <= 0)
        {
            onListChanged?.Invoke();
            return;
        }

        onAmountChanged?.Invoke();
    }
}
