namespace ModdableWeather.Components;

public class RainSoakedEffectApplier : TickableComponent, IDeletableEntity
{

#nullable disable
    RainWeather rainWeather;
    Enterer enterer;
    SoakedEffectService soakedEffectService;
    NeedManager needManager;
    bool available;
#nullable enable

    [Inject]
    public void Inject(
        RainWeather rainWeather,
        SoakedEffectService soakedEffectService)
    {
        this.rainWeather = rainWeather;
        this.soakedEffectService = soakedEffectService;
    }

    public void Awake()
    {
        needManager = GetComponentFast<NeedManager>();
        enterer = GetComponentFast<Enterer>();

        if (!needManager || !enterer)
        {
            available = false;
            enabled = false;
            return;
        }

        available = true;
    }

    public override void StartTickable()
    {
        if (!available) { return; }

        rainWeather.OnWeatherActiveChanged += OnRainChanged;
        OnRainChanged(rainWeather, rainWeather.Active, true);
    }

    private void OnRainChanged(IModdedWeather weather, bool active, bool onLoad)
    {
        if (this)
        {
            enabled = active;
        }
    }

    public override void Tick()
    {
        if (!rainWeather.Enabled || enterer.IsInside) { return; }

        foreach (var eff in soakedEffectService.Effects)
        {
            needManager.ApplyEffect(eff);
        }
    }

    public void DeleteEntity()
    {
        rainWeather.OnWeatherActiveChanged -= OnRainChanged;
    }
}
