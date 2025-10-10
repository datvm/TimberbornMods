namespace ScientificProjects.Services.BaseMod;

public class ModProjectsWorkplaceDescriber : IWorkplaceUpgradeDescriber
{
    public IEnumerable<EntityEffectDescription> DescribeEffects(WorkplaceTrackerComponent comp, DescribeEffectsParameters parameters)
    {
        var t = parameters.CharacterUpgradeDescriber.T;

        var activeBuilderUpgrade = GetActiveBuilderUpgrade(comp, parameters);
        if (activeBuilderUpgrade is not null)
        {
            yield return new(
                t.T("LV.SP.CarryBuilderUpgrade"),
                t.T("LV.SP.CarryBuilderUpgradeBuildingDesc", activeBuilderUpgrade.Spec.Parameters[0]));
        }
    }

    ScientificProjectInfo? GetActiveBuilderUpgrade(WorkplaceTrackerComponent comp, DescribeEffectsParameters parameters)
    {
        return comp.IsBuilderWorkplace
            && parameters.ActiveProjects.TryGetValue(ScientificProjectsUtils.CarryBuilderUpgradeId, out var upgrade)
                ? upgrade
                : null;
    }

}
