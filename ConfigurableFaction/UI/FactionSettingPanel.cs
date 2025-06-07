
namespace ConfigurableFaction.UI;

public class FactionSettingPanel(
    FactionOptionsProvider optionsProvider,
    FactionInfoService info,
    IContainer container,
    ILoc t
) : VisualElement
{

#nullable disable
    FactionOptions options;
    FactionInfo currFaction;
#nullable enable

    SettingsFilter filter = new();
    VisualElement? pnlFilters;
    ImmutableArray<FactionInfo> otherFactions = [];

    FactionBuildingsPanel? buildingsPanel;

    void Populate()
    {
        pnlFilters ??= PopulateFilter();
        var parent = this;

        parent.Clear();

        parent.Add(pnlFilters);

        buildingsPanel = container.GetInstance<FactionBuildingsPanel>();
        parent.Add(buildingsPanel.Init(options, currFaction, otherFactions));
    }

    VisualElement PopulateFilter()
    {
        var el = new VisualElement().SetMarginBottom(10);

        el.AddLabel(t.T("LV.CFac.Filters").Bold().Color(TimberbornTextColor.Solid));

        var row = el.AddRow();
        row.AddTextField("Filter", OnFilterTextChanged).SetFlexGrow(1);

        row.AddToggle(t.T("LV.CFac.FilterCheck"), onValueChanged: v => OnFilterCheckedChanged(true, v))
            .SetFlexShrink(0).SetValueWithoutNotify(filter.ShowChecked);
        row.AddToggle(t.T("LV.CFac.FilterUncheck"), onValueChanged: v => OnFilterCheckedChanged(false, v))
            .SetFlexShrink(0).SetValueWithoutNotify(filter.ShowUnchecked);

        return el;
    }

    void OnFilterTextChanged(string text)
    {
        filter.Keyword = text;
        buildingsPanel?.SetFilter(filter);
    }

    void OnFilterCheckedChanged(bool forChecked, bool show)
    {
        if (forChecked)
        {
            filter.ShowChecked = show;
        }
        else
        {
            filter.ShowUnchecked = show;
        }

        buildingsPanel?.SetFilter(filter);
    }

    public void SetFaction(FactionInfo faction)
    {
        var id = faction.Spec.Id;
        options = optionsProvider.FactionOptions[id];

        currFaction = faction;
        otherFactions = [..info.FactionsInfo!.Factions
            .Where(q => q.Spec.Id != id)];

        Populate();
    }
}
