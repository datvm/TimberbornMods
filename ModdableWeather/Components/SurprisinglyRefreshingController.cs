namespace ModdableWeather.Components;

public class SurprisinglyRefreshingController : BaseComponent, IDeletableEntity
{

#nullable disable
    WaterSourceContamination waterSourceContamination;
    SurprisinglyRefreshingWeather refreshingWeather;
#nullable enable

    [Inject]
    public void Inject(SurprisinglyRefreshingWeather refreshingWeather)
    {
        this.refreshingWeather = refreshingWeather;
    }

    public void Awake()
    {
        waterSourceContamination = GetComponentFast<WaterSourceContamination>();
    }

    public void Start()
    {
        refreshingWeather.OnWeatherActiveChanged += OnWeatherChanged;
        if (refreshingWeather.Active)
        {
            OnWeatherChanged(refreshingWeather, true, true);
        }
    }

    void OnWeatherChanged(IModdableWeather weather, bool active, bool onLoad)
    {
        if (active)
        {
            waterSourceContamination.SetContamination(0f);
        }
        else
        {
            waterSourceContamination.ResetContamination();
        }
    }

    public void DeleteEntity()
    {
        refreshingWeather.OnWeatherActiveChanged -= OnWeatherChanged;
    }
}
