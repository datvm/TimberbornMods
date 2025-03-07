namespace ScientificProjects.Buffs;

public class CarryingBuffInst : CommonProjectBeaverBuffInstance<CarryingBuff>
{

    protected override IBuffEffect? CreateBuffEffect(ScientificProjectInfo info)
        => info.Spec.HasSteps ? null : new CarryingBuffEff(info.Spec.Parameters[0], info.Spec.DisplayName, t);

}

public class CarryingBuilderBuffInst : CommonProjectBuffInstance<CarryingBuff>
{
    protected override IBuffEffect? CreateBuffEffect(ScientificProjectInfo info)
        => CreateLevelEffect(info, (v, n) => new CarryingBuffEff(v, n, t));

    protected override IBuffTarget[] CreateTargets() => [new BeaverBuilderBuffTarget(buffables, ev)];
}