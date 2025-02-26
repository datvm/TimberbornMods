
namespace BuffDebuffDemo.Buffs;

public class PositiveBuffInstance : SimpleFloatBuffInstance<PositiveBuff>
{
    public override bool IsBuff { get; protected set; }
    public override IEnumerable<IBuffTarget> Targets { get; protected set; }
    public override IEnumerable<IBuffEffect> Effects { get; protected set; }
}
