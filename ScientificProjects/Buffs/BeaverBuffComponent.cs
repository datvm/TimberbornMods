namespace ScientificProjects.Buffs;

public class BeaverBuffComponent : BaseComponent
{
    const string SpeedBonusId = "MovementSpeed";
    const string WorkEffBonusId = "WorkingSpeed";

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
            case MoveSpeedBuffInst ms:
                ApplySpeedBuff(ms, false);
                break;
            case WorkEffBuffInst we:
                ApplyWorkEffBuff(we, false);
                break;
        }
    }

    private void Buffable_OnBuffRemoved(object sender, BuffInstance e)
    {
        if (!e.Active) { return; }

        switch (e)
        {
            case MoveSpeedBuffInst ms:
                ApplySpeedBuff(ms, true);
                break;
            case WorkEffBuffInst we:
                ApplyWorkEffBuff(we, true);
                break;
        }
    }

    void ApplySpeedBuff(MoveSpeedBuffInst instance, bool negative)
    {
        var effects = instance.Effects.Where(q => q is MoveSpeedBuffEff).Cast<MoveSpeedBuffEff>();
        foreach (var e in effects)
        {
            bonus.AddBonus(SpeedBonusId, e.Value * (negative ? -1 : 1));
        }
    }

    void ApplyWorkEffBuff(WorkEffBuffInst instance, bool negative)
    {
        var effects = instance.Effects.Where(q => q is WorkEffBuffEff).Cast<WorkEffBuffEff>();
        foreach (var e in effects)
        {
            bonus.AddBonus(WorkEffBonusId, e.Value * (negative ? -1 : 1));
        }
    }

}
