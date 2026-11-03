namespace ThePostOffice.UI;

[BindTransient]
public class OrderItemElement(
    IGoodService goods,
    ILoc t,
    PostOfficeService offices,
    VisualElementInitializer veInit
) : VisualElement
{
    const int MaxAmount = 99;

    NineSliceIntegerField amountField = null!;
    Label totalLabel = null!;
    bool built;
    bool updating;

    public string Id { get; private set; } = "";
    public int Amount { get; private set; }

    public event Action? Changed;
    public event Action? Removed;

    public void SetGood(string id, int amount)
    {
        Id = id;
        Amount = Math.Clamp(amount, 1, MaxAmount);
        if (!built)
        {
            Build();
            built = true;
        }

        Paint();
    }

    public void SetAmount(int value)
    {
        var next = Math.Clamp(value, 1, MaxAmount);
        var changed = next != Amount;
        Amount = next;
        if (built)
        {
            updating = true;
            amountField.SetValueWithoutNotify(next);
            updating = false;
            Paint();
        }

        if (changed)
        {
            Changed?.Invoke();
        }
    }

    void Build()
    {
        this.SetAsRow().AlignItems().SetMarginBottom(5);
        this.AddIconSpan().SetGood(goods, Id, showName: true).SetFlexGrow(0).SetFlexShrink(0);
        this.AddChild().SetMarginAuto();
        this.AddGameLabel(t.T("LV.TPO.UnitWeight", offices.ItemWeight(Id))).SetFlexShrink(0);

        amountField = this.AddIntField(changeCallback: OnField)
            .SetWidth(48)
            .SetFlexShrink(0)
            .SetMargin(left: 4);
        amountField.SetValueWithoutNotify(Amount);
        this.AddPlusButton(size: UiBuilder.GameButtonSize.Small)
            .AddAction(() => SetAmount(Amount + 1))
            .SetMargin(left: 4)
            .SetFlexShrink(0);
        this.AddMinusButton(size: UiBuilder.GameButtonSize.Small)
            .AddAction(Decrease)
            .SetMargin(left: 4)
            .SetFlexShrink(0);
        totalLabel = this.AddGameLabel().SetWidth(64).SetFlexShrink(0).SetMargin(left: 5);
        this.Initialize(veInit);
        Paint();
    }

    void OnField(int value)
    {
        if (updating)
        {
            return;
        }

        SetAmount(value);
    }

    void Decrease()
    {
        if (Amount <= 1)
        {
            Removed?.Invoke();
            return;
        }

        SetAmount(Amount - 1);
    }

    void Paint()
    {
        totalLabel.text = t.T("LV.TPO.LineWeight", offices.ItemWeight(Id) * Amount);
    }
}
