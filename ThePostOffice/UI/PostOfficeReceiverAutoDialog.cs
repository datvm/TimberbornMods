namespace ThePostOffice.UI;

[BindTransient]
public class PostOfficeReceiverAutoDialog(
    IContainer container,
    PanelStack panelStack,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdowns,
    IGoodService goods,
    GoodDescriber describer,
    ILoc t,
    PostOfficeService offices
) : DialogBoxElement
{
    readonly List<PostOfficeAutoRule> result = [];
    PostOfficeAmountEditor editor = null!;
    Label summary = null!;
    Label warning = null!;
    Button confirm = null!;
    int capacity;

    void Initialize(int buildingCapacity, IReadOnlyList<PostOfficeAutoRule> current)
    {
        capacity = buildingCapacity;
        var parent = Content;
        SetTitle(t.T("LV.TPO.ConfigureAuto"));
        AddCloseButton();
        SetDialogSize(width: 640, height: 520);

        editor = new PostOfficeAmountEditor(Mailable(), goods, describer, t, container, veInit, dropdowns);
        editor.Build(parent, () =>
        {
            summary = parent.AddGameLabel().SetMarginBottom(5);
            warning = parent.AddGameLabel(t.T("LV.TPO.TooHeavy")).SetMarginBottom(5);
            confirm = parent.AddGameButtonPadded(t.T("LV.TPO.Confirm"), Confirm, paddingY: 3).SetMarginBottom(8);
        }, t.T("LV.TPO.ReceiverAutoHelp"));
        editor.SetRules(current);
        editor.Changed += Refresh;
        Refresh();
    }

    public async Task<IReadOnlyList<PostOfficeAutoRule>?> ShowAsync(int buildingCapacity, IReadOnlyList<PostOfficeAutoRule> current)
    {
        Initialize(buildingCapacity, current);
        return await ShowAsync(veInit, panelStack) ? result : null;
    }

    void Confirm()
    {
        var next = editor.Read();
        if (!Fits(next))
        {
            return;
        }

        result.Clear();
        result.AddRange(next);
        OnUIConfirmed();
    }

    void Refresh()
    {
        var next = editor.Read();
        var fits = Fits(next);
        summary.text = t.T("LV.TPO.KeepWeight", Weight(next), capacity);
        warning.SetDisplay(!fits);
        confirm.SetEnabled(fits);
    }

    bool Fits(IReadOnlyList<PostOfficeAutoRule> next)
    {
        if (Weight(next) <= capacity)
        {
            return true;
        }

        return next.Count == 1
            && next[0].Target == 1
            && offices.ItemWeight(next[0].GoodId) > capacity;
    }

    int Weight(IReadOnlyList<PostOfficeAutoRule> next)
    {
        var sum = 0;
        foreach (var rule in next)
        {
            sum += offices.ItemWeight(rule.GoodId) * rule.Target;
        }

        return sum;
    }

    List<GoodSpec> Mailable()
    {
        List<GoodSpec> catalog = [];
        foreach (var id in goods.Goods)
        {
            if (!goods.HasGood(id))
            {
                continue;
            }

            var spec = goods.GetGood(id);
            if (spec.GoodType == "Liquid" || spec.Weight <= 0)
            {
                continue;
            }

            catalog.Add(spec);
        }

        catalog.Sort((left, right) => string.Compare(
            left.DisplayName.Value,
            right.DisplayName.Value,
            StringComparison.CurrentCultureIgnoreCase));
        return catalog;
    }
}
