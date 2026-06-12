namespace ModdableTimberborn.ModdableWaterSource;

public class ModdableWaterSourceService(
    DefaultEntityTracker<ModdableWaterSourceComponent> waterSources
) : ILoadableSingleton
{

    public IReadOnlyCollection<ModdableWaterSourceComponent> AllWaterSources => waterSources.Entities;
    public IEnumerable<ModdableWaterSourceComponent> FreshWaterSources => AllWaterSources.Where(ws => !ws.IsContaminationSource);
    public IEnumerable<ModdableWaterSourceComponent> BadwaterSources => AllWaterSources.Where(ws => ws.IsContaminationSource);

    public StackableValue<bool> GlobalImmuneToDrought { get; } = new(false);
    public StackableValue<bool> GlobalImmuneToBadtide { get; } = new(false);
    
    public StackableValue<float> GlobalStrengthMultiplier { get; } = new(1f);
    public StackableValue<float> GlobalContaminationDelta { get; } = new(0f);

#nullable disable
    StackableModifier<bool> globalImmuneToDroughtModifier, globalImmuneToBadtideModifier;
    StackableModifier<float> globalStrengthMultiplierModifier, globalContaminationDeltaModifier;
#nullable enable

    public void Load()
    {
        globalImmuneToDroughtModifier = new(nameof(GlobalImmuneToDrought), (int)ModifierPriority.Additive, ctx => ctx.Value = ctx.Value || GlobalImmuneToDrought.CalculateThisFrame());
        globalImmuneToBadtideModifier = new(nameof(GlobalImmuneToBadtide), (int)ModifierPriority.Additive, ctx => ctx.Value = ctx.Value || GlobalImmuneToBadtide.CalculateThisFrame());
        globalStrengthMultiplierModifier = new(nameof(GlobalStrengthMultiplier), (int)ModifierPriority.Multiplicative, ctx => ctx.Value *= GlobalStrengthMultiplier.CalculateThisFrame());
        globalContaminationDeltaModifier = new(nameof(GlobalContaminationDelta), (int)ModifierPriority.Multiplicative, ctx => ctx.Value += GlobalContaminationDelta.CalculateThisFrame());

        waterSources.OnEntityRegistered += OnWaterSource;
    }

    void OnWaterSource(ModdableWaterSourceComponent obj)
    {
        obj.ImmuneToDrought.AddOrUpdate(globalImmuneToDroughtModifier);
        obj.ImmuneToBadtide.AddOrUpdate(globalImmuneToBadtideModifier);
        obj.StrengthMultiplier.AddOrUpdate(globalStrengthMultiplierModifier);
        obj.ContaminationDelta.AddOrUpdate(globalContaminationDeltaModifier);
    }

}
