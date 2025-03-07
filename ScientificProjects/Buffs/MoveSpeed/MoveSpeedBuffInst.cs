
namespace ScientificProjects.Buffs;

public class MoveSpeedBuffInst : CommonProjectBeaverBuffInstance<MoveSpeedBuff>
{

    protected override IBuffEffect? CreateBuffEffect(ScientificProjectInfo info) 
        => CreateFlatEffect(info, (v, n) => new MoveSpeedBuffEff(v, n, t));

}
