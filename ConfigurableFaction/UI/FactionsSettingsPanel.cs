namespace ConfigurableFaction.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class FactionsSettingsPanel(
    UserSettingsUIControllerScope controllerScope,
    ILoc t,
    DialogService dialogService,
    IContainer container,
    DataAggregatorService aggregator
) : VisualElement
{

#nullable disable
    VisualElement listContainer;
    Toggle chkClear;
    TextField txtFilter;
#nullable enable

    readonly UserSettingsUIController controller = controllerScope.Controller;
    FactionCollectionPanel[] factionPanels = [];

    public void Initialize()
    {
        controller.ListChanged += ReloadData;
        controller.StateChanged += ReloadState;

        chkClear = this.AddToggle(t.T("LV.CF.ClearFaction"), onValueChanged: controller.ChangeClear);
        this.AddGameLabel(t.T("LV.CF.ClearFactionDesc")).SetMarginBottom(10);

        this.AddGameLabel(t.T("LV.CF.Filter"));
        txtFilter = this.AddTextField(changeCallback: OnFilterChanged).SetMarginBottom(10);

        var buttons = this.AddRow().SetMarginBottom(10);
        buttons.AddGameButtonPadded(t.T("LV.CF.ExpandAll"), onClick: () => ExpandAll(true)).SetMarginRight(5);
        buttons.AddGameButtonPadded(t.T("LV.CF.CollapseAll"), onClick: () => ExpandAll(false));

        buttons.AddGameButtonPadded(t.T("LV.CF.DeselectAll"), onClick: DeselectAll)
            .SetMarginLeftAuto();

        listContainer = this.AddChild();
    }

    void ReloadData()
    {
        var lst = listContainer;

        var curr = controller.Current;
        chkClear.SetValueWithoutNotify(curr.Clear);
        
        var currId = curr.FactionId;
        var factions = aggregator.Factions.Items;
        factionPanels = new FactionCollectionPanel[factions.Length];

        FactionCollectionPanel? currPanel = null;

        lst.Clear();
        for (int i = 0; i < factionPanels.Length; i++)
        {
            var f = factions[i];

            var panel = factionPanels[i] = container.GetInstance<FactionCollectionPanel>();
            panel.Initialize(f.Id);
            lst.Add(panel);

            if (f.Id == currId)
            {
                currPanel = panel;
            }
        }

        if (currPanel is null)
        {
            throw new Exception("Current faction panel not found");
        }
        currPanel.parent.Insert(0, currPanel);

        OnFilterChanged(txtFilter.value);
    }

    void ReloadState()
    {
        foreach (var panel in factionPanels)
        {
            panel.UpdateEntriesStates();
        }
    }

    void ExpandAll(bool expand)
    {
        Stack<VisualElement> stack = new([this]);

        while (stack.Count > 0)
        {
            var el = stack.Pop();

            if (el is CollapsiblePanel cp)
            {
                cp.SetExpand(expand);
            }

            foreach (var child in el.Children())
            {
                stack.Push(child);
            }
        }
    }

    async void DeselectAll()
    {
        if (!await dialogService.ConfirmAsync("LV.CF.DeselectAllConfirm", localized: true)) { return; }
        controller.DeselectAll();
    }

    void OnFilterChanged(string filter)
    {
        filter = filter.Trim().ToLower();

        foreach (var p in factionPanels)
        {
            p.SetFilter(filter);
        }
    }

}
