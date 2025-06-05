namespace ModdableWeather.Components;

public class MonsoonWaterStrengthModifier : BaseComponent, IWaterStrengthModifier, IDeletableEntity
{

#nullable disable
    WaterSource waterSource;
    MonsoonWeather monsoon;
#nullable enable

    bool active;

    [Inject]
    public void Inject(MonsoonWeather monsoon)
    {
        this.monsoon = monsoon;
    }

    public void Awake()
    {
        waterSource = GetComponentFast<WaterSource>();
    }

    public void Start()
    {
        monsoon.OnWeatherActiveChanged += Monsoon_OnWeatherActiveChanged;
        if (monsoon.Active)
        {
            Monsoon_OnWeatherActiveChanged(monsoon, true, true);
        }
    }

    private void Monsoon_OnWeatherActiveChanged(IModdedWeather weather, bool active, bool onLoad)
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

    public float GetStrengthModifier() => monsoon.WaterModifier;

    public void DeleteEntity()
    {
        monsoon.OnWeatherActiveChanged -= Monsoon_OnWeatherActiveChanged;
    }
}
