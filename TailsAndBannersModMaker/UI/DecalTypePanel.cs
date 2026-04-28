namespace TailsAndBannersModMaker.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class DecalTypePanel(
    IContainer container,
    ILoc t,
    DialogService diag
) : CollapsiblePanel
{

#nullable disable
    ModMakerDecalInfo info;
    string type;
    VisualElement groupElementList;
#nullable enable

    public DecalTypePanel Initialize(string type, ModMakerDecalInfo info)
    {
        this.info = info;
        this.type = type;

        SetTitle(t.TDecal(type));
        SetExpand(false);
        this.SetMarginBottom(10).SetBorder();

        var parent = Container;

        var buttons = parent.AddRow().AlignItems().SetMarginBottom();
        buttons.AddGameButtonPadded(t.T("LV.TBMM.AddGroup"), AddGroup);

        groupElementList = parent.AddChild();

        info.DefaultGroup.Name = t.T("LV.TBMM.DefaultGroupName");
        AddGroupUI(info.DefaultGroup);

        foreach (var group in info.Groups) // Should not have any but just in case
        {
            AddGroupUI(group);
        }

        return this;
    }

    void AddGroup()
    {
        var grp = new DecalGroupInfo();
        info.Groups.Add(grp);
        AddGroupUI(grp);
    }

    void AddGroupUI(DecalGroupInfo group)
    {
        var pnl = container.GetInstance<DecalGroupPanel>().Initialize(group, type);
        pnl.OnRemoveRequested += OnGroupRemoveRequested;

        groupElementList.Add(pnl);
    }

    async void OnGroupRemoveRequested(object sender, EventArgs e)
    {
        if (!await diag.ConfirmAsync("LV.TBMM.DeleteGroupConfirm", true)) { return; }

        var pnl = (DecalGroupPanel)sender;

        info.Groups.Remove(pnl.DecalGroupInfo);
        pnl.RemoveFromHierarchy();
    }
}
