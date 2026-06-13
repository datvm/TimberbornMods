using ModdableTimberborn.ModdableWaterSource;

namespace BeaverChronicles.Services.Buffs;

[BindSingleton]
public class WaterSourceEntityBuffService(
    ISingletonLoader loader,
    IDayNightCycle dayNightCycle,
    DefaultEntityTracker<ModdableWaterSourceComponent> waterSources
) : EntityBuffService<WaterSourceBuffStatus>(loader, dayNightCycle)
{
    protected override string SaveId => nameof(WaterSourceEntityBuffService);

    public void AddOrUpdateWaterSourceBuff(WaterSourceBuffStatus buff, float? days) => AddOrUpdate(buff, days);
    public void RemoveWaterSourceBuff(string buffId) => Remove(buffId);

    protected override void OnLoaded()
    {
        waterSources.OnEntityRegistered += ApplyBuffs;

        foreach (var waterSource in waterSources.Entities)
        {
            ApplyBuffs(waterSource);
        }
    }

    protected override void Apply(WaterSourceBuffStatus buff)
    {
        foreach (var waterSource in GetWaterSources(buff.EntityIds))
        {
            ApplyBuff(waterSource, buff);
        }
    }

    protected override void RemoveBuff(WaterSourceBuffStatus buff)
    {
        foreach (var waterSource in GetWaterSources(buff.EntityIds))
        {
            waterSource.ImmuneToDrought.Remove(buff.Id);
            waterSource.ImmuneToBadtide.Remove(buff.Id);
            waterSource.StrengthMultiplier.Remove(buff.Id);
            waterSource.ContaminationDelta.Remove(buff.Id);
            waterSource.GetStatusDescription().RemoveStatus(buff.Id);
        }
    }

    void ApplyBuffs(ModdableWaterSourceComponent waterSource)
    {
        var entityId = waterSource.GetEntityId();
        foreach (var buff in Buffs.Values)
        {
            if (buff.EntityIds is null || buff.EntityIds.Contains(entityId))
            {
                ApplyBuff(waterSource, buff);
            }
        }
    }

    static void ApplyBuff(ModdableWaterSourceComponent waterSource, WaterSourceBuffStatus buff)
    {
        var effects = buff.Effects;

        if (effects.ImmuneToDrought is { } immuneToDrought)
        {
            waterSource.ImmuneToDrought.AddOrUpdate(new(buff.Id, (int)ModifierPriority.Additive, ctx => ctx.Value = ctx.Value || immuneToDrought));
        }

        if (effects.ImmuneToBadtide is { } immuneToBadtide)
        {
            waterSource.ImmuneToBadtide.AddOrUpdate(new(buff.Id, (int)ModifierPriority.Additive, ctx => ctx.Value = ctx.Value || immuneToBadtide));
        }

        if (effects.StrengthMultiplier is { } strengthMultiplier)
        {
            waterSource.StrengthMultiplier.AddOrUpdate(new(buff.Id, (int)ModifierPriority.Multiplicative, ctx => ctx.Value *= strengthMultiplier));
        }

        if (effects.ContaminationDelta is { } contaminationDelta)
        {
            waterSource.ContaminationDelta.AddOrUpdate(new(buff.Id, (int)ModifierPriority.Additive, ctx => ctx.Value += contaminationDelta));
        }

        waterSource.GetStatusDescription().AddStatus(buff);
    }

    IEnumerable<ModdableWaterSourceComponent> GetWaterSources(IReadOnlyCollection<Guid>? entityIds)
    {
        foreach (var waterSource in waterSources.Entities)
        {
            if (entityIds is null || entityIds.Contains(waterSource.GetEntityId()))
            {
                yield return waterSource;
            }
        }
    }
}
