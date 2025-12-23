namespace ModdableWeathers.Components;

public class WeatherSoakEffectApplier(
    SoakedEffectService soakedEffectService,
    SoakEffectApplierService soakEffectApplierService
) : TickableComponent, IAwakableComponent, IDeletableEntity
{

#nullable disable
    Enterer enterer;
    NeedManager needManager;
#nullable enable

    public void Awake()
    {
        needManager = GetComponent<NeedManager>();
        enterer = GetComponent<Enterer>();
        
        if (!needManager || !enterer)
        {
            DisableComponent();
            return;
        }

        if (!soakEffectApplierService.ApplyingToOutsiders)
        {
            DisableComponent();
        }
        soakEffectApplierService.OnApplyingToOutsidersChanged += OnApplyingChanged;
    }

    void OnApplyingChanged(bool obj)
    {
        if (obj)
        {
            EnableComponent();
        }
        else
        {
            DisableComponent();
        }
    }

    public void DeleteEntity()
    {
        soakEffectApplierService.OnApplyingToOutsidersChanged -= OnApplyingChanged;
    }

    public override void Tick()
    {
        if (enterer.IsInside) { return; }

        foreach (var eff in soakedEffectService.Effects)
        {
            needManager.ApplyEffect(eff);
        }
    }
}
