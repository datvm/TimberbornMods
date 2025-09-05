namespace EarthquakeWeather.Services;

public class EarthquakeInjuryService(
    EarthquakeStrikeService earthquakeStrikeService,
    BeaverPopulation beaverPopulation,
    BotPopulation botPopulation,
    EarthquakeWeatherSettings s,
    EarthquakeNotificationService notf
) : ILoadableSingleton
{
    const string InjuryId = "Injury";

    static readonly InstantEffect InjuryEffect = new(InjuryId, -1f, 1);

    public void Load()
    {
        earthquakeStrikeService.OnEarthquakeAreaStrike += OnEarthquakeHitArea;
    }

    private void OnEarthquakeHitArea(EarthquakeArea area)
    {
        var chance = s.InjuryChance.Value;
        if (chance <= 0) { return; }

        var beavers = FindEntitiesInArea(beaverPopulation._beaverCollection.Beavers, area);
        foreach (var b in beavers)
        {
            if (ShouldInjure())
            {
                Injure(b);
            }
        }

        var bots = FindEntitiesInArea(botPopulation._bots, area);
        foreach (var b in bots)
        {
            if (ShouldInjure())
            {
                Damage(b);
            }
        }

        bool ShouldInjure() => chance == 100 || UnityEngine.Random.Range(0, 100) < chance;
    }

    void Injure(BeaverSpec b)
    {
        var needManager = b.GetComponentFast<NeedManager>();
        if (!needManager || !needManager.HasNeed(InjuryId)) { return; }

        needManager.ApplyEffect(InjuryEffect);
        notf.LogInjuredBeaver();
    }

    void Damage(BotSpec b)
    {
        var deteriorable = b.GetComponentFast<Deteriorable>();
        if (!deteriorable) { return; }

        deteriorable._currentDeterioration -= deteriorable._deteriorableSpec.DeteriorationInDays * (s.BotDurabilityLoss.Value / 100f);
        notf.LogDamagedBot();
    }

    List<T> FindEntitiesInArea<T>(IEnumerable<T> entities, in EarthquakeArea area) where T : BaseComponent
    {
        var result = new List<T>();
        var (c, r, _) = area;
        var r2 = r * r;
        
        foreach (var e in entities)
        {
            var pos = CoordinateSystem.WorldToGrid(e.GameObjectFast.transform.position).XY();            
            var d2 = (pos - c).sqrMagnitude;
            if (d2 <= r2)
            {
                result.Add(e);
            }
        }

        return result;
    }

}
