namespace PowerCopy.UI;

public class ObjectSelectionDialog(
    VisualElementInitializer veInit,
    PanelStack panelStack,
    ILoc t,
    ObjectListingService objectListingService,
    EntitySelectionService entitySelectionService
) : DialogBoxElement
{
    const string CopyingComponentsName = "CopyingComponents";
    const string CheckBoxName = "CheckBox";

    ObjectListingDetailedResult list = null!;
    Button btnCopy = null!;
    int currCount = 0;

    void Init()
    {
        SetDialogPercentSize(null, .8f);
        SetTitle(t.T("LV.PC.PowerCopyTitle"));
        AddCloseButton();

        var parent = Content;

        var header = parent.AddRow().AlignItems();

        currCount = list.SelectedComponents.Count();
        btnCopy = header.AddMenuButton(onClick: OnUIConfirmed);
        SetCopyButtonText();

        header.AddToggle(t.T("LV.PC.ShowCopyingComponents"), onValueChanged: ToggleComponentsList).SetMarginLeftAuto();

        parent.AddGameButtonPadded(t.T("LV.PC.ToggleAll"), onClick: ToggleAll).SetMarginBottom().SetMarginLeftAuto();

        foreach (var (dc, objs) in list.ObjectsByDistricts)
        {
            var grpEl = parent.AddCollapsiblePanel(
                t.T("LV.PC.DistrictTitle", dc ? dc!.DistrictName : t.T("LV.PC.CopyLocations_NoDistrict"), objs.Length))
                .SetMarginBottom();

            var grpParent = grpEl.Container;
            foreach (var obj in objs)
            {
                var row = grpParent.AddRow().AlignItems(Align.FlexStart).SetMarginBottom(5);
                
                row.AddImage(obj.Entity.GetComponent<LabeledEntity>().Image).SetSize(32).SetMarginRight(5);
                row.AddToggle(obj.Name, name: CheckBoxName, onValueChanged: v => OnCheckedChanged(obj, v)).SetValueWithoutNotify(obj.Checked);

                row.AddChild().SetMarginLeftAuto();

                var dups = string.Join(Environment.NewLine, obj.Duplicables.Select(d => d.GetType().Name));
                var lblDups = row.AddGameLabel(dups, name: CopyingComponentsName).SetDisplay(false).SetMarginRight(5);
                lblDups.style.unityTextAlign = TextAnchor.MiddleRight;

                row.AddGameButtonPadded(t.T("LV.PC.Focus"), onClick: () => Focus(obj.Entity));
            }
        }
    }

    void Focus(EntityComponent comp)
    {
        entitySelectionService.FocusOnSelectable(comp.GetComponent<SelectableObject>());
    }

    void OnCheckedChanged(ObjectListingDetailedEntry entry, bool v)
    {
        if(entry.Checked == v ) { return; }

        entry.Checked = v;
        currCount += v ? 1 : -1;
        SetCopyButtonText();
    }

    void SetCopyButtonText() => btnCopy.text = t.T("LV.PC.ConfirmCopy", currCount);

    void ToggleComponentsList(bool show)
    {
        foreach (var el in Content.Query(name: CopyingComponentsName).Build())
        {
            el.SetDisplay(show);
        }
    }

    void ToggleAll()
    {
        var count = list.AllComponents.Count();
        if (count == 0) { return; }

        var first = list.AllComponents.First();
        var target = !first.Checked;

        foreach (var c in list.AllComponents)
        {
            c.Checked = target;
        }

        foreach (var t in Content.Query<Toggle>(name: CheckBoxName).Build())
        {
            t.SetValueWithoutNotify(target);
        }

        currCount = target ? count : 0;
        SetCopyButtonText();
    }

    public async Task<EntityComponent[]> ShowAsync(ObjectListingQuery query)
    {
        list = objectListingService.QueryDetailedObjects(query);
        Init();
        
        var confirmed = await ShowAsync(veInit, panelStack);

        return confirmed
            ? [.. list.SelectedComponents]
            : [];
    }

}
