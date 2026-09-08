namespace ConfigurableFaction.UI;

public class FactionBuildingRow : FactionPrefabRow
{
    protected override HashSet<NormalizedPrefabSpec> ExistingList => options.ExistingBuildingsPrefabName;
    protected override HashSet<string> GetList() => options.Buildings;
    string? planterGroupName;

    protected override void OnPrefabSelected(NormalizedPrefabSpec spec, bool add)
    {
        if (add)
        {
            optionsService.AddBuilding(options, spec.Path);
        }
        else
        {
            optionsService.RemoveBuilding(options, spec.Path);
        }
    }

    public override FactionPrefabRow SetPrefab(NormalizedPrefabSpec prefab, ILoc t, FactionOptions options, FactionOptionsService optionsService)
    {
        planterGroupName = optionsService.GetPlantableResourceGroup(prefab.PrefabSpec);
        
        var result = base.SetPrefab(prefab, t, options, optionsService);

        if (planterGroupName is not null)
        {
            SetLockFunc((_, isChecked) => !isChecked && options.LockedOutPlantGroup.Contains(planterGroupName));
        }

        return result;
    }

    protected override string GetName(LabeledEntitySpec spec, ILoc t)
    {
        return base.GetName(spec, t)
            + (planterGroupName is null ? "" : $" ({t.T("LV.CFac.PlanterGroup", planterGroupName)})");
    }
}
