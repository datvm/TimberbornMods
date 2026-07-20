namespace BrainPowerSPs.Components;

public class SparePowerDescriber(SparePowerToScienceConverter service) : BaseComponent, IEntityEffectDescriber, IInitializableEntity
{

#nullable disable
    Manufactory manufactory;
#nullable enable

    bool isProducingScience;

    public void InitializeEntity()
    {
        manufactory = GetComponent<Manufactory>();
        manufactory.RecipeChanged += OnRecipeChanged;
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
