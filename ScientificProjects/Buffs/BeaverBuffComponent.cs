
namespace ScientificProjects.Buffs;

public class BeaverBuffComponent : BaseComponent
{
    const string SpeedBonusId = "MovementSpeed";

    BuffableComponent buffable = null!;
    BonusManager bonus = null!;

    public void Awake()
    {
        buffable = GetComponentFast<BuffableComponent>();
        bonus = GetComponentFast<BonusManager>();
    }

    public void Start()
    {
        buffable.OnBuffAdded += Buffable_OnBuffAdded;
        buffable.OnBuffRemoved += Buffable_OnBuffRemoved;
    }

    private void Buffable_OnBuffAdded(object sender, BuffInstance e)
    {
        if (!e.Active) { return; }

        switch (e)
        {
            case MovementSpeedUpgradeBuffInstance ms:
                ApplySpeedBuff(ms, false);
                break;
        }
    }

    private void Buffable_OnBuffRemoved(object sender, BuffInstance e)
    {
        if (!e.Active) { return; }

        switch (e)
        {
            case MovementSpeedUpgradeBuffInstance ms:
                ApplySpeedBuff(ms, true);
                break;
        }
    }

    void ApplySpeedBuff(MovementSpeedUpgradeBuffInstance instance, bool negative)
    {
        var effects = instance.Effects.Where(q => q is MovementSpeedBuffEffect).Cast<MovementSpeedBuffEffect>();
        foreach (var e in effects)
        {
            bonus.AddBonus(SpeedBonusId, e.Speed * (negative ? -1 : 1));
        }
    }

}
