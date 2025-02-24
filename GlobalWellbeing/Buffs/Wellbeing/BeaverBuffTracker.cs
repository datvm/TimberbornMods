namespace GlobalWellbeing.Buffs.Wellbeing;

public class BeaverBuffTracker : BaseComponent
{
    public const string SpeedBonusId = "MovementSpeed";

    BuffableComponent buffable = null!;
    BonusManager bonusManager = null!;

    public void Awake()
    {
        buffable = GetComponentFast<BuffableComponent>();
        bonusManager = GetComponentFast<BonusManager>();
    }

    public void Start()
    {
        buffable.OnBuffAdded += Buffable_OnBuffAdded;
        buffable.OnBuffRemoved += Buffable_OnBuffRemoved;
    }

    private void Buffable_OnBuffRemoved(object sender, BuffInstance e)
    {
        switch (e)
        {
            case WellBeingBuffInstance b:
                AddRemoveWellbeingBuff(b, false);
                break;
            case HighscoreWellbeingBuffInstance b:
                AddRemoveHighscoreBuff(b, false);
                break;
        }
    }

    private void Buffable_OnBuffAdded(object sender, BuffInstance e)
    {
        switch (e)
        {
            case WellBeingBuffInstance b:
                AddRemoveWellbeingBuff(b, true);
                break;
            case HighscoreWellbeingBuffInstance b:
                AddRemoveHighscoreBuff(b, true);
                break;
        }
    }

    void AddRemoveHighscoreBuff(HighscoreWellbeingBuffInstance b, bool add)
    {
        if (add)
        {
            bonusManager.AddBonus(SpeedBonusId, b.Effect.Speed);
        }
        else
        {
            bonusManager.RemoveBonus(SpeedBonusId, b.Effect.Speed);
        }
    }

    void AddRemoveWellbeingBuff(WellBeingBuffInstance b, bool add)
    {
        foreach (var ef in b.WellBeingEffect)
        {
            if (add)
            {
                bonusManager.AddBonus(ef.Current.Name, ef.Current.Value);
            }
            else
            {
                bonusManager.RemoveBonus(ef.Current.Name, ef.Current.Value);
            }
        }
    }

}
