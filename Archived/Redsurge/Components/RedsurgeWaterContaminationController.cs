namespace Redsurge.Components;

public class RedsurgeWaterContaminationController : BaseComponent, IDeletableEntity
{
#nullable disable
    WaterSourceContamination waterSourceContamination;
    RedsurgeWeather redsurgeWeather;
#nullable enable

    const float ContaminationValue = 1.0f;

    [Inject]
    public void Inject(RedsurgeWeather redsurgeWeather)
    {
        this.redsurgeWeather = redsurgeWeather;
    }

    public void Awake()
    {
        waterSourceContamination = GetComponentFast<WaterSourceContamination>();
    }

    public void Start()
    {
        redsurgeWeather.OnWeatherActiveChanged += OnWeatherChanged;
        if (redsurgeWeather.Active)
        {
            OnWeatherChanged(redsurgeWeather, true, true);
        }
    }

    private void OnWeatherChanged(IModdedWeather weather, bool active, bool onLoad)
    {
        if (active)
        {
            waterSourceContamination.SetContamination(ContaminationValue);
        }
        else
        {
            waterSourceContamination.ResetContamination();
        }
    }

    public void DeleteEntity()
    {
        redsurgeWeather.OnWeatherActiveChanged -= OnWeatherChanged;
    }
}
