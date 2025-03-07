namespace ScientificProjects.Buffs;

public class WorkEffBuffInst : CommonProjectBeaverBuffInstance<WorkEffBuff>
{

    protected override IBuffEffect CreateBuffEffect(ScientificProjectInfo info) 
        => CreateFlatOrLevelEffect(info, (v, n) => new WorkEffBuffEff(v, n, t));

}
