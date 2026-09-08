namespace ConfigurableFaction.UI;

public class FactionPlantsPanel(
    FactionOptionsService optionsService,
    ILoc t,
    DialogBoxShower diag
) : GroupPanel, IFactionItemsPanel
{
    readonly List<FactionPlantRow> plants = [];

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

        SetHeader(t.T("LV.CFac.Plants"));
        InitList(otherFactions);

        return this;
    }

    void InitList(ImmutableArray<FactionInfo> otherFactions)
    {
        var parent = Content;
        plants.Clear();

        foreach (var faction in otherFactions)
        {
            var factionRow = parent.AddChild().SetMarginBottom(10);
            factionRow.AddLabel(t.T("LV.CFac.FromFaction", faction.Spec.DisplayName.Value).Bold().Color(TimberbornTextColor.Solid));

            foreach (var plantGroup in faction.Plantables)
            {
                var grpRow = factionRow.AddChild().SetMarginBottom(5);

                var grpHeader = grpRow.AddRow().AlignItems();
                grpHeader.AddLabel(t.T("LV.CFac.PlanterGroup", plantGroup.Group));

                foreach (var plant in plantGroup.Plants)
                {
                    var row = grpRow.AddChild<FactionPlantRow>();
                    row.SetPrefab(plant, t, options, optionsService);
                    row.OnValueChanged += OnPlantRowChanged;
                    plants.Add(row);
                }
            }
        }
    }

    private void OnPlantRowChanged(NormalizedPrefabSpec prefab, bool add)
    {
        OnItemChanged?.Invoke();

        if (!add) { return; }

        var plantSpec = prefab.PrefabSpec.GetComponentFast<PlantableSpec>();
        if (!plantSpec) { return; }

        var grp = plantSpec.ResourceGroup;
        if (options.LockedOutPlantGroup.Contains(grp)) { return; }

        diag.Create()
            .SetMessage(t.T("LV.CFac.NeedPlanter", grp))
            .Show();
    }

    public void SetFilter(SettingsFilter filter)
    {
        foreach (var row in plants)
        {
            row.Filter(filter);
        }
    }

    public void RefreshItems()
    {
        foreach (var row in plants)
        {
            row.Value = options.Plantables.Contains(row.Data.Path);
        }
    }

}
