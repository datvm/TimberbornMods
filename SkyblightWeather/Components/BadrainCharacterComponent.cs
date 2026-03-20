namespace SkyblightWeather.Components;

[AddTemplateModule2(typeof(Enterer))]
public class BadrainCharacterComponent(BadrainCharacterApplier applier) : TickableComponent, IAwakableComponent, IEntityEffectDescriber, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BadrainCharacterComponent));
    static readonly PropertyKey<int> TicksInRainKey = new("TicksInRain");

#nullable disable
    Enterer enterer;
#nullable enable

    public bool IsBot { get; private set; }

    Deteriorable? botDurability;
    Contaminable? contaminable;

    int ticksInRain;

    public void Awake()
    {
        enterer = GetComponent<Enterer>();
        botDurability = this.GetComponentOrNull<Deteriorable>();
        contaminable = this.GetComponentOrNull<Contaminable>();
        IsBot = botDurability is not null;

        DisableComponent();
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
    {
        if (!Enabled || (IsBot && enterer.IsInside)) { return null; }

        return new(t.T("LV.MWSb.Badrain"), IsBot
            ? t.T("LV.MWSb.BadrainBotStatus", applier.BotDamagerPercent)
            : t.T("LV.MWSb.BadrainBeaverStatus", dayNightCycle.TicksToHours(ticksInRain), GetSicknessChance()));
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(TicksInRainKey))
        {
            ticksInRain = s.Get(TicksInRainKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (ticksInRain <= 0) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(TicksInRainKey, ticksInRain);
    }

    public override void Tick()
    {
        if (enterer.IsInside) { return; }

        if (IsBot)
        {
            if (applier.BotDamagePerTick.HasValue)
            {
                Damage(applier.BotDamagePerTick.Value);
            }
        }
        else
        {
            ticksInRain++;
        }
    }

    void Damage(float perc)
    {
        if (!botDurability) { return; }

        var damage = perc * botDurability!._deteriorableSpec.DeteriorationInDays;
        botDurability._currentDeterioration -= damage;
    }

    public void AfflictBadRainSickness()
    {
        if (IsBot || !contaminable) { return; }
        
        var chance = GetSicknessChance();
        var hit = chance >= 1f;
        if (chance > 0 && !hit)
        {
            hit = UnityEngine.Random.Range(0, 1f) < chance;
        }

        if (hit)
        {
            contaminable!.Contaminate();
        }

        ticksInRain = 0;
    }

    float GetSicknessChance()
    {
        var (Min, Max, Delta) = applier.BeaverSicknessValues;

        if (ticksInRain < Min) { return 0f; }
        if (ticksInRain >= Max) { return 1f; }

        return (ticksInRain - Min) / Delta;
    }

}
