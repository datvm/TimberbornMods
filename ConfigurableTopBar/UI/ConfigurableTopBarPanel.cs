namespace ConfigurableTopBar.UI;

public class ConfigurableTopBarPanel(
    IContainer container,
    TopBarConfigProvider provider,
    ILoc t,
    DialogService diag
) : VisualElement
{

    VisualElement list = null!;

    public ConfigurableTopBarPanel Init()
    {
        provider.ClearSelectingGoods();
        this.AddMenuButton(t.T("LV.CTB.AddGroup"), onClick: AddGroup).SetMarginBottom();
        list = this.AddChild();

        provider.GroupsChanged += ShowGroups;
        provider.SelectingGoodsEmptyChanged += OnSelectingGoodsChanged;

        ShowGroups();
        return this;
    }

    void OnSelectingGoodsChanged(bool isEmppty)
    {
        foreach (var el in list.Children().OfType<EditableGoodGroupPanel>())
        {
            el.SetSelectingEmpty(isEmppty);
        }
    }

    void AddGroup()
    {
        var item = new EditableGoodGroupSpec(GetNewId(), GoodSpriteProvider.QuestionMarkPath, t.T("LV.CTB.NewGroup"));
        provider.Groups.Add(item);

        ShowGroups();
    }

    public void ShowGroups()
    {
        var expanding = GetExpandingIds().ToHashSet();

        list.Clear();

        foreach (var grp in provider.Groups.Append(provider.SpecialGroup))
        {
            var panel = list.AddChild(container.GetInstance<EditableGoodGroupPanel>).Init(grp);
            panel.SetExpand(expanding.Contains(grp.Id));
        }
    }

    public async void Reset()
    {
        provider.Reset();

        await Task.Delay(1);
        diag.Alert("LV.CTB.ResetNotice", true);
    }

    IEnumerable<string> GetExpandingIds()
    {
        foreach (var child in list.Children())
        {
            if (child is EditableGoodGroupPanel panel && panel.Expand)
            {
                yield return panel.Spec.Id;
            }
        }
    }

    string GetNewId()
    {
        var id = 1;

        while (provider.Groups.Any(q => q.Id == "NewId" + id))
        {
            id++;
        }

        return "NewId" + id;
    }

}
