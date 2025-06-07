namespace ConfigurableFaction.UI;

public class FactionBuildingsPanel(
    FactionOptionsService optionsService,
    FactionInfoService info,
    ILoc t
) : GroupPanel
{

    readonly HashSet<string> existingPath = [];
    readonly HashSet<string> normalizedName = [];

    readonly List<FactionBuildingPanelRow> buildings = [];

#nullable disable
    FactionOptions options;
#nullable enable

    public FactionBuildingsPanel Init(
        FactionOptions options,
        FactionInfo currFaction,
        ImmutableArray<FactionInfo> otherFactions
    )
    {
        this.options = options;

        SetHeader(t.T("LV.CFac.Buildings"));

        InitCurrFaction(currFaction);
        InitList(otherFactions);

        return this;
    }

    void InitCurrFaction(FactionInfo curr)
    {
        existingPath.AddRange(curr.Buildings.Select(q => q.Path));
        normalizedName.AddRange(curr.Buildings.Select(q => q.NormalizedName));
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
                if(toolGrp.ToolGroupSpec.Icon)
                {
                    img.sprite = toolGrp.ToolGroupSpec.Icon;
                }
                toolGrpHeader.AddLabel(t.T(toolGrp.ToolGroupSpec.NameLocKey));

                foreach (var building in toolGrp.Prefabs)
                {
                    var row = grpRow.AddChild<FactionBuildingPanelRow>()
                        .SetBuilding(building, t);
                    row.Value = options.Buildings.Contains(building.Path);

                    buildings.Add(row);
                }
            }
        }
    }

    public void SetFilter(SettingsFilter filter)
    {
        foreach (var row in buildings)
        {
            row.Visible = filter.Match(row.Text, row.Value);
        }
    }

}

public class FactionBuildingPanelRow : VisualElement
{

    public NormalizedPrefabSpec Spec { get; private set; }

    readonly Toggle chkEnabled;

    public string Text { get; private set; } = "";

    public bool Value
    {
        get => chkEnabled.value;
        set => chkEnabled.SetValueWithoutNotify(value);
    }

    public bool Visible
    {
        get => this.IsDisplayed();
        set => this.SetDisplay(value);
    }

    public FactionBuildingPanelRow()
    {
        chkEnabled = this.AddToggle().SetWidthPercent(100);
    }

    public FactionBuildingPanelRow SetBuilding(NormalizedPrefabSpec spec, ILoc t)
    {
        Spec = spec;

        var label = spec.PrefabSpec.GetComponentFast<LabeledEntitySpec>();
        Text = chkEnabled.text = t.T(label.DisplayNameLocKey);

        return this;
    }

}