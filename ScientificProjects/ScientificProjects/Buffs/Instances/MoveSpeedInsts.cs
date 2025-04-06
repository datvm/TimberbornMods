using ScientificProjects.Extensions;

namespace ScientificProjects.Buffs;

public class MoveSpeedUpgradeBuffInst : CommonProjectBeaverBuffInstance<ResearchProjectsBuff>
{
    protected override IBuffEffect? CreateBuffEffect(ScientificProjectInfo info)
        => CreateFlatEffect(info, (v, n) => new MoveSpeedBuffEff(v, n, t));

    protected override string GetBuffName(ILoc t) => "LV.SP.MoveSpeedUpgradeBuff".T(t);
    protected override string GetBuffDescription(ILoc t) => "LV.SP.MoveSpeedUpgradeBuffDesc".T(t);    
}

public class MoveSpeedBuffEff(float value, string name, ILoc t) : SimpleValueBuffEffect<float>(value)
{
    protected override string? GetDescription(float value)
        => string.Format("LV.SP.MoveSpeedUpgradeBuffEff".T(t), value, name);

}
