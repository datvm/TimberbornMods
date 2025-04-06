
using ScientificProjects.Extensions;

namespace ScientificProjects.Buffs;

public class CarryingUpgradeBuffInst : CommonProjectBeaverBuffInstance<ResearchProjectsBuff>
{
    protected override IBuffEffect? CreateBuffEffect(ScientificProjectInfo info)
        => CreateFlatEffect(info, (v, n) => new CarryingBuffEff(v, n, t));

    protected override string GetBuffName(ILoc t) => "LV.SP.CarryUpgradeBuff".T(t);
    protected override string GetBuffDescription(ILoc t) => "LV.SP.CarryUpgradeBuffDesc".T(t);
}

public class CarryingBuilderUpgradeBuffInst : CommonProjectBeaverBuffInstance<ResearchProjectsBuff>
{
    protected override IBuffEffect? CreateBuffEffect(ScientificProjectInfo info)
        => CreateLevelEffect(info, (v, n) => new CarryingBuffEff(v, n, t));

    BotPopulation botPops = null!;
    [Inject]
    public void Inject(BotPopulation botPops)
    {
        this.botPops = botPops;
    }

    protected override IBuffTarget[] CreateTargets()
    {
        return [new GlobalBuilderBuffTarget(ev, beaverPops, botPops)];
    }

    protected override string GetBuffDescription(ILoc t) => "LV.SP.CarryBuilderBuffDesc".T(t);
    protected override string GetBuffName(ILoc t) => "LV.SP.CarryBuilderBuff".T(t);
}

public class CarryingBuffEff(float value, string name, ILoc t) : SimpleValueBuffEffect<float>(value)
{
    protected override string? GetDescription(float value)
        => string.Format("LV.SP.CarryUpgradeBuffEff".T(t), value, name);

}
