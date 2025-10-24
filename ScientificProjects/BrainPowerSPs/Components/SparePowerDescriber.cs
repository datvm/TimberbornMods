namespace BrainPowerSPs.Components;

public class SparePowerDescriber : BaseComponent, IEntityEffectDescriber
{

#nullable disable
    SparePowerToScienceConverter service;
    Manufactory manufactory;
#nullable enable

    bool isProducingScience;

    [Inject]
    public void Inject(SparePowerToScienceConverter service)
    {
        this.service = service;
    }

    public void Start()
    {
        manufactory = GetComponentFast<Manufactory>();
        manufactory.ProductionRecipeChanged += OnRecipeChanged;
        OnRecipeChanged(this, EventArgs.Empty);
    }

    void OnRecipeChanged(object sender, EventArgs e)
    {
        isProducingScience = manufactory.HasCurrentRecipe && manufactory.CurrentRecipe.ProducesSciencePoints;
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle) 
        => isProducingScience && service.Unlocked
            ? new(
                t.T("LV.BPSP.SparePowerScience"),
                t.T("LV.BPSP.SparePowerDesc", service.AccumulatedScience, service.RemainingHour)
            )
            : null;
}
