namespace GlobalWellbeing.Buffs;

public readonly struct WellBeingEffectStatPair(WellBeingEffectStat current, WellBeingEffectStat? next)
{
    public WellBeingEffectStat Current => current;
    public WellBeingEffectStat? Next => next;
}

public readonly struct WellBeingEffectStat(string name, float value, int wellbeing)
{
    public string Name => name;
    public float Value => value;
    public int Wellbeing => wellbeing;
    public string Percent => (value > 0 ? "+" : "") + (value * 100).ToString("F0");
}

public class WellBeingEffect : IBuffEffect
{
    private readonly WellBeingEffectStatPair pair;

    public WellBeingEffectStat Current => pair.Current;
    public WellBeingEffectStat? Next => pair.Next;

    public string Description { get; }

    public long Id { get; set; }

    public WellBeingEffect(ILoc t, WellBeingEffectStatPair pair)
    {
        this.pair = pair;

        var current = pair.Current;
        var next = pair.Next;

        var name = t.T("Bonus." + current.Name);
        if (next is null)
        {
            Description = string.Format(t.T("LV.GW.BonusEnd"),
                name,
                current.Percent
            );
        }
        else
        {
            Description = string.Format(t.T("LV.GW.Bonus"),
                name,
                current.Percent,
                next.Value.Wellbeing,
                next.Value.Percent
            );
        }
    }

    public void CleanUp() { }
    public void Init() { }
    public void UpdateEffect() { }
}
