namespace GlobalWellbeing.Buffs.HighScore;

public class HighscoreWellbeingBuffEffect(ILoc t, float speed) : IBuffEffect
{
    public float Speed => speed;

    public string? Description { get; } = t.T("LV.GW.NewScoreBuffMs", speed.ToString("+#%;-#%;0%"));
    public long Id { get; set; }

    public void CleanUp() { }
    public void Init() { }
    public void UpdateEffect() { }

}
