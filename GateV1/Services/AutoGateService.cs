namespace GateV1.Services;

public class AutoGateService(
    EventBus eb,
    EntityRegistry entities
) : ILoadableSingleton
{

    public void Load()
    {
        eb.Register(this);
    }

    [OnEvent]
    public void OnHazardousWeather(HazardousWeatherStartedEvent e)
    {
        var isBadtide = e.HazardousWeather.Id == BadtideWeather.BadtideWeatherKey.Name;

        foreach (var entity in entities.Entities)
        {
            var gate = entity.GetComponent<GateComponent>();
            if (!gate || gate.Closed) { continue; }

            if (gate.AutoCloseHaz || (isBadtide && gate.AutoCloseBadtide))
            {
                gate.ToggleClosedState(true);
            }
        }
    }

}
