namespace ScientificProjects.Buffs;

public class WorkEffUpgradeBuffInst : CommonProjectBeaverBuffInstance<ResearchProjectsBuff>
{
    protected override IBuffEffect? CreateBuffEffect(ScientificProjectInfo info)
        => CreateFlatOrLevelEffect(info, (v, n) => new WorkEffBuffEff(v, n, t));

    protected override string GetBuffName(ILoc t) => "LV.SP.WorkEffUpgradeBuff".T(t);
    protected override string GetBuffDescription(ILoc t) => "LV.SP.WorkEffUpgradeBuffDesc".T(t);
}

public class WorkEffBuffEff(float value, string name, ILoc t) : SimpleValueBuffEffect<float>(value)
{
    protected override string? GetDescription(float value)
        => string.Format("LV.SP.WorkEffUpgradeBuffEff".T(t), value, name);

}
