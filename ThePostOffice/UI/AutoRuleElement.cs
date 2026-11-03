namespace ThePostOffice.UI;

[BindTransient]
public class AutoRuleElement(
    IGoodService goods,
    ILoc t,
    PostOfficeService offices,
    VisualElementInitializer veInit
) : VisualElement
{
    public const int UnitWidth = 56;
    public const int FieldWidth = 64;
    public const int TotalWidth = 72;
    public const int RemoveWidth = 75;

    const int MaxAmount = 9999;

    NineSliceIntegerField targetField = null!;
    NineSliceIntegerField marginField = null!;
    Label totalLabel = null!;
    bool built;
    bool updating;

    public string Id { get; private set; } = "";
    public int Target { get; private set; }
    public int Margin { get; private set; }

    public event Action? Changed;
    public event Action? Removed;

    public static void AddHeaders(VisualElement parent, ILoc t, string amountLabel)
    {
        var labels = parent.AddRow().AlignItems().SetMarginBottom(2);
        labels.AddGameLabel(t.T("LV.TPO.Good")).SetFlexGrow();
        labels.AddChild().SetWidth(UnitWidth).SetFlexShrink(0);
        labels.AddGameLabel(amountLabel).SetWidth(FieldWidth).SetFlexShrink(0).SetMargin(left: 4);
        labels.AddGameLabel(t.T("LV.TPO.Margin")).SetWidth(FieldWidth).SetFlexShrink(0).SetMargin(left: 4);
        labels.AddChild().SetWidth(TotalWidth).SetFlexShrink(0).SetMargin(left: 5);
        labels.AddChild().SetWidth(RemoveWidth).SetFlexShrink(0).SetMargin(left: 5);
    }

    public void SetRule(string id, int target, int margin)
    {
        Id = id;
        Target = Math.Clamp(target, 1, MaxAmount);
        Margin = Math.Clamp(margin, 0, MaxAmount);
        if (!built)
        {
            Build();
            built = true;
            return;
        }

        updating = true;
        targetField.SetValueWithoutNotify(Target);
        marginField.SetValueWithoutNotify(Margin);
        updating = false;
        Paint();
    }

    public void SetTarget(int value)
    {
        var next = Math.Clamp(value, 1, MaxAmount);
        if (next == Target)
        {
            return;
        }

        Target = next;
        if (!built)
        {
            return;
        }

        updating = true;
        targetField.SetValueWithoutNotify(Target);
        updating = false;
        Paint();
        Changed?.Invoke();
    }

    void Build()
    {
        this.SetAsRow().AlignItems().SetMarginBottom(5);
        this.AddIconSpan().SetGood(goods, Id, showName: true).SetFlexGrow(0).SetFlexShrink(0);
        this.AddChild().SetMarginAuto();
        this.AddGameLabel(t.T("LV.TPO.UnitWeight", offices.ItemWeight(Id)))
            .SetWidth(UnitWidth)
            .SetFlexShrink(0);
        targetField = this.AddIntField(changeCallback: OnTarget)
            .SetWidth(FieldWidth)
            .SetFlexShrink(0)
            .SetMargin(left: 4);
        targetField.isDelayed = true;
        targetField.SetValueWithoutNotify(Target);
        marginField = this.AddIntField(changeCallback: OnMargin)
            .SetWidth(FieldWidth)
            .SetFlexShrink(0)
            .SetMargin(left: 4);
        marginField.isDelayed = true;
        marginField.SetValueWithoutNotify(Margin);
        totalLabel = this.AddGameLabel().SetWidth(TotalWidth).SetFlexShrink(0).SetMargin(left: 5);
        this.AddGameButtonPadded(t.T("LV.TPO.Remove"), () => Removed?.Invoke(), paddingY: 2)
            .SetMargin(left: 5)
            .SetFlexShrink(0);
        this.Initialize(veInit);
        Paint();
    }

    void OnTarget(int value)
    {
        if (updating)
        {
            return;
        }

        var next = Math.Clamp(value, 1, MaxAmount);
        Target = next;
        updating = true;
        targetField.SetValueWithoutNotify(next);
        updating = false;
        Paint();
        Changed?.Invoke();
    }

    void OnMargin(int value)
    {
        if (updating)
        {
            return;
        }

        var next = Math.Clamp(value, 0, MaxAmount);
        Margin = next;
        updating = true;
        marginField.SetValueWithoutNotify(next);
        updating = false;
        Changed?.Invoke();
    }

    void Paint()
    {
        totalLabel.text = t.T("LV.TPO.LineWeight", offices.ItemWeight(Id) * Target);
    }
}
