namespace ScientificProjects.Buffs;

public class WorkEffBuffInst : CommonProjectBeaverBuffInstance<WorkEffBuff>
{

    protected override IBuffEffect CreateBuffEffect(ScientificProjectInfo info)
    {
        var s = info.Spec;
        var speed = s.Parameters[0];
        var name = s.DisplayName;

        if (s.HasSteps)
        {
            speed *= info.TodayLevel;
            name += $" ({info.TodayLevel})";
        }

        return new WorkEffBuffEff(speed, name, t);
    }
}
