namespace EarthquakeWeather.Services;

public class EarthquakeConstructionDamageService(
    Earthquake earthquake,
    EntityRegistry entityRegistry,
    ILoc t,
    EarthquakeNotificationService earthquakeNotification
) : ILoadableSingleton
{

    public void Load()
    {
        earthquake.OnEarthquakeHit += OnEarthquakeHit;
    }

    void OnEarthquakeHit()
    {
        var count = 0;

        foreach (var entity in entityRegistry.Entities)
        {
            var construction = entity.GetComponentFast<ConstructionSite>();
            if (!construction) { continue; }

            var progress = construction.BuildTimeProgress;
            if (progress >= 1f || progress <= 0f) { continue; }

            count++;
            construction.SetBuildTimeProgress(0f);
        }

        if (count > 0)
        {
            earthquakeNotification.AppendNotification(t.T("LV.EQ.ConstructionDamageNotf", count));
        }
    }
}
