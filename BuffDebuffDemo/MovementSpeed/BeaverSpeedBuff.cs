
namespace BuffDebuffDemo.MovementSpeed;

public class BeaverSpeedBuff(Loc t) : SimpleFloatBuff<BeaverSpeedBuffInstance>
{
    public override string Name => t.T("");
    public override string Description { get; }
}

public class BeaverSpeedBuffInstance : SimpleFloatBuffInstance<BeaverSpeedBuffInstance>
{

}

public class SpeedBuffEffect(float value) : SimpleValueBuffEffect<float>(value);