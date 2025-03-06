
namespace ScientificProjects.Buffs;

public class MoveSpeedBuffInst : CommonProjectBeaverBuffInstance<MoveSpeedBuff>
{

    protected override IBuffEffect CreateBuffEffect(ScientificProjectInfo info) 
        => new MoveSpeedBuffEff(info.Spec.Parameters[0], info.Spec.DisplayName, t);

}
