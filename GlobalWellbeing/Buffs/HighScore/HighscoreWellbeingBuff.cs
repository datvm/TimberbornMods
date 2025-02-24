namespace GlobalWellbeing.Buffs.HighScore;

public class HighscoreWellbeingBuff(ILoc t, ISingletonLoader loader, IBuffService buffs, WellbeingService wellbeing) : SimpleBuff(loader, buffs), ITickableSingleton
{
    const float BonusHours = 24f;

    static readonly SingletonKey SaveKey = new("HighscoreWellbeingBuff");
    static readonly PropertyKey<int> HighscoreKey = new("Highscore");
    
    readonly IBuffService buffs = buffs;

    protected override SingletonKey SingletonKey => SaveKey;

    public override string Name { get; protected set; } = t.T("LV.GW.NewScoreBuff");
    public override string Description { get; protected set; } = t.T("LV.GW.NewScoreBuffDesc");

    public int HighScore { get; private set; }

    public void Tick()
    {
        if (wellbeing.AverageGlobalWellbeing <= HighScore) { return; }

        var prev = HighScore;
        HighScore = wellbeing.AverageGlobalWellbeing;

        ApplyBonus(prev, HighScore);
    }

    void ApplyBonus(int prev, int high)
    {
        var bonusTime = BonusHours * (HighScore - prev);
        var bonusSpeed = HighScore / 100f;

        var existingInstance = buffs.GetInstances<HighscoreWellbeingBuffInstance>().FirstOrDefault();
        if (existingInstance is not null)
        {
            bonusTime += existingInstance.RemainingTime;
            buffs.Remove(existingInstance);
        }

        var instance = buffs.CreateBuffInstance<HighscoreWellbeingBuff, HighscoreWellbeingBuffInstance, HighscoreWellbeingValue>(
            this,
            new(bonusTime, bonusSpeed));
        buffs.Apply(instance);
    }

    protected override void LoadSingleton(IObjectLoader loader)
    {
        base.LoadSingleton(loader);
        HighScore = loader.Get(HighscoreKey);
    }

    protected override void SaveSingleton(IObjectSaver saver)
    {
        base.SaveSingleton(saver);
        saver.Set(HighscoreKey, HighScore);
    }

}
