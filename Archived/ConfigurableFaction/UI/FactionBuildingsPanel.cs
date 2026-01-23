namespace ConfigurableFaction.UI;

public class FactionBuildingsPanel(
    FactionOptionsService optionsService,
    ILoc t
) : GroupPanel, IFactionItemsPanel
{

    readonly List<FactionBuildingRow> buildings = [];

#nullable disable
    FactionOptions options;
#nullable enable

    public event Action? OnItemChanged;

    public IFactionItemsPanel Init(
        FactionOptions options,
        ImmutableArray<FactionInfo> otherFactions
    )
    {
        this.options = options;

        SetHeader(t.T("LV.CFac.Buildings"));
        InitList(otherFactions);

        return this;
    }

    void InitList(ImmutableArray<FactionInfo> otherFactions)
    {
        var parent = Content;
        buildings.Clear();

        foreach (var faction in otherFactions)
        {
            var factionRow = parent.AddChild().SetMarginBottom(10);
            factionRow.AddLabel(t.T("LV.CFac.FromFaction", faction.Spec.DisplayName.Value).Bold().Color(TimberbornTextColor.Solid));

            foreach (var toolGrp in faction.BuildingByToolGroups)
            {
                var grpRow = factionRow.AddChild().SetMarginBottom(5);

                var toolGrpHeader = grpRow.AddRow().AlignItems();
                var img = toolGrpHeader.AddImage().SetSize(40).SetMarginRight();
                if (toolGrp.ToolGroupSpec.Icon)
                {
                    img.sprite = toolGrp.ToolGroupSpec.Icon;
                }
                toolGrpHeader.AddLabel(t.T(toolGrp.ToolGroupSpec.NameLocKey));

                foreach (var building in toolGrp.Prefabs)
                {
                    var row = grpRow.AddChild<FactionBuildingRow>();
                    row.SetPrefab(building, t, options, optionsService);
                    
                    row.OnValueChanged += (_, _) => OnItemChanged?.Invoke();

                    buildings.Add(row);
                }
            }
        }
    }

    public void SetFilter(SettingsFilter filter)
    {
        foreach (var row in buildings)
        {
            row.Filter(filter);
        }
    }

    public void RefreshItems()
    {
        foreach (var row in buildings)
        {
            row.Value = options.Buildings.Contains(row.Data.Path);
        }
    }
}
