namespace ConfigurableFaction.UI;

public class FactionSettingPanel(
    FactionOptionsProvider optionsProvider,
    FactionInfoService info,
    IContainer container,
    ILoc t
) : VisualElement
{
    public static readonly ImmutableArray<Type> PanelTypes =
    [
        typeof(FactionBuildingsPanel),
        typeof(FactionPlantsPanel),
        typeof(FactionGoodsPanel),
        typeof(FactionNeedsPanel),
    ];

#nullable disable
    FactionOptions options;
#nullable enable

    readonly SettingsFilter filter = new();
    VisualElement? pnlFilters;
    ImmutableArray<FactionInfo> otherFactions = [];

    ImmutableArray<IFactionItemsPanel> panels = [];

    void Populate()
    {
        pnlFilters ??= PopulateFilter();
        var container = this;

        container.Clear();

        container.Add(pnlFilters);
        PopulatePanels(container);
    }

    VisualElement PopulateFilter()
    {
        var el = new VisualElement().SetMarginBottom(10);

        el.AddLabel(t.T("LV.CFac.Filters").Bold().Color(TimberbornTextColor.Solid));

        var row = el.AddRow().SetMarginBottom(5);
        row.AddTextField("Filter", OnFilterTextChanged).SetFlexGrow(1);

        row.AddToggle(t.T("LV.CFac.FilterCheck"), onValueChanged: v => OnFilterCheckedChanged(true, v))
            .SetFlexShrink(0).SetValueWithoutNotify(filter.ShowChecked);
        row.AddToggle(t.T("LV.CFac.FilterUncheck"), onValueChanged: v => OnFilterCheckedChanged(false, v))
            .SetFlexShrink(0).SetValueWithoutNotify(filter.ShowUnchecked);

        el.AddToggle(t.T("LV.CFac.HideSimilar"), onValueChanged: OnFilterSimilarChanged)
            .SetValueWithoutNotify(true);

        return el;
    }

    void PopulatePanels(VisualElement parent)
    {
        panels = [.. PanelTypes.Select(type =>
        {
            var panel = (IFactionItemsPanel)container.GetInstance(type);
            panel.Init(options, otherFactions);

            panel.OnItemChanged += OnPanelItemChanged;

            var panelEl = (GroupPanel)panel;
            panelEl.ToggleCollapse(true);

            parent.Add(panelEl);
            return panel;
        })];

        foreach (var panel in panels)
        {
            panel.SetFilter(filter);
            panel.RefreshItems();
        }
    }

    void OnFilterTextChanged(string text)
    {
        filter.Keyword = text;
        OnFilterChanged();
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

        OnFilterChanged();
    }

    void OnFilterSimilarChanged(bool enabled)
    {
        filter.HideSimilar = enabled;
        OnFilterChanged();
    }

    void OnFilterChanged()
    {
        foreach (var panel in panels)
        {
            panel.SetFilter(filter);
        }
    }

    void OnPanelItemChanged()
    {
        foreach (var panel in panels)
        {
            panel.RefreshItems();
        }
    }

    public void SetFaction(FactionInfo faction)
    {
        var id = faction.Spec.Id;
        options = optionsProvider.FactionOptions[id];

        otherFactions = [..info.FactionsInfo!.Factions
            .Where(q => q.Spec.Id != id)];

        Populate();
    }
}
