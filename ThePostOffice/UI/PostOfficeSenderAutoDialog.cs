namespace ThePostOffice.UI;

[BindTransient]
public class PostOfficeSenderAutoDialog(
    IContainer container,
    PanelStack panelStack,
    VisualElementInitializer veInit,
    DropdownItemsSetter dropdowns,
    IGoodService goods,
    GoodDescriber describer,
    ILoc t
) : DialogBoxElement
{
    readonly List<PostOfficeAutoRule> result = [];
    PostOfficeRuleEditor editor = null!;

    void Initialize(IReadOnlyList<PostOfficeAutoRule> current)
    {
        var parent = Content;
        SetTitle(t.T("LV.TPO.ConfigureAuto"));
        AddCloseButton();
        SetDialogSize(width: 640, height: 520);

        editor = new PostOfficeRuleEditor(Mailable(), goods, describer, t, container, veInit, dropdowns);
        editor.Build(parent, Confirm, t.T("LV.TPO.SenderAutoHelp"));
        editor.SetRules(current);
    }

    public async Task<IReadOnlyList<PostOfficeAutoRule>?> ShowAsync(IReadOnlyList<PostOfficeAutoRule> current)
    {
        Initialize(current);
        return await ShowAsync(veInit, panelStack) ? result : null;
    }

    void Confirm()
    {
        result.Clear();
        result.AddRange(editor.Read());
        OnUIConfirmed();
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
