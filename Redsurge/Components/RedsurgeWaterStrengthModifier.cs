namespace Redsurge.Components;

public class RedsurgeWaterStrengthModifier : BaseComponent, IWaterStrengthModifier, IDeletableEntity
{
#nullable disable
    WaterSource waterSource;
    RedsurgeWeather redsurge;
#nullable enable

    bool active;

    [Inject]
    public void Inject(RedsurgeWeather redsurge)
    {
        this.redsurge = redsurge;
    }

    public void Awake()
    {
        waterSource = GetComponentFast<WaterSource>();
    }

    public void Start()
    {
        redsurge.OnWeatherActiveChanged += Redsurge_OnWeatherActiveChanged;
        if (redsurge.Active)
        {
            Redsurge_OnWeatherActiveChanged(redsurge, true, true);
        }
    }

    private void Redsurge_OnWeatherActiveChanged(IModdedWeather weather, bool active, bool onLoad)
    {
        if (this.active == active) { return; }
        this.active = active;

        if (active)
        {
            waterSource.AddWaterStrengthModifier(this);
        }
        else
        {
            waterSource.RemoveWaterStrengthModifier(this);
        }
    }

    public float GetStrengthModifier() => redsurge.WaterModifier;

    public void DeleteEntity()
    {
        redsurge.OnWeatherActiveChanged -= Redsurge_OnWeatherActiveChanged;
    }
}
