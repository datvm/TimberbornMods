namespace ScientificProjects.Services.BaseMod;

public class ModProjectsCharacterDescriber : ICharacterUpgradeDescriber
{

    public IEnumerable<EntityEffectDescription> DescribeEffects(
        CharacterTrackerComponent comp,
        DescribeEffectsParameters parameters
    )
    {
        // Work
        var workEff = ICharacterUpgradeDescriber.DescribeProjects(
            comp, parameters,
            ScientificProjectsUtils.WorkEffUpgradeIds, 0,
            "LV.SP.WorkEffUpgradeBuff", "LV.SP.WorkEffUpgradeBuffDesc",
            ModUpgradeListener.WorkEffBonusId, true
        );
        if (workEff is not null)
        {
            yield return workEff.Value;
        }

        // Speed
        var movespeedEff= ICharacterUpgradeDescriber.DescribeProjects(
            comp, parameters,
            ScientificProjectsUtils.MoveSpeedUpgradeIds, 0,
            "LV.SP.MoveSpeedUpgradeBuff", "LV.SP.MoveSpeedUpgradeBuffDesc",
            ModUpgradeListener.MoveSpeedBonusId, true
        );
        if (movespeedEff is not null)
        {
            yield return movespeedEff.Value;
        }

        // Carrying
        var t = parameters.CharacterUpgradeDescriber.T;
        if (comp.BonusTracker!.TryGetBonus(ModUpgradeListener.CarryBonusId, out var carryingBonus))
        {
            parameters.ActiveProjects.TryGetValue(ScientificProjectsUtils.CarryUpgradeId, out var beaverUpgrade);
            parameters.ActiveProjects.TryGetValue(ScientificProjectsUtils.CarryBuilderUpgradeId, out var builderUpgrade);

            List<string> desc = [t.T("LV.SP.CarryUpgradeBuffDesc", carryingBonus.Value.Bonuses[0].MultiplierDelta)];

            if (comp.CharacterType.IsBeaver() && beaverUpgrade is not null)
            {
                desc.Add(beaverUpgrade.DescribeEffect(t, 0, true));
            }

            if (comp.Worker.IsBuilder() && builderUpgrade is not null)
            {
                desc.Add(builderUpgrade.DescribeEffect(t, 0, true));
            }

            yield return new(t.T("LV.SP.CarryUpgradeBuff"), string.Join(Environment.NewLine, desc));
        }
    }

}
