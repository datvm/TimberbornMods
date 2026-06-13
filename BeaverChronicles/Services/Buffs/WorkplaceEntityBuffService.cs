namespace BeaverChronicles.Services.Buffs;

[BindSingleton]
public class WorkplaceEntityBuffService(
    ISingletonLoader loader,
    IDayNightCycle dayNightCycle,
    WorkplaceTracker workplaces,
    FindEntityHelper findEntityHelper
) : EntityBuffService<WorkplaceBuffStatus>(loader, dayNightCycle)
{
    protected override string SaveId => nameof(WorkplaceEntityBuffService);

    public void AddOrUpdateWorkplaceBuff(WorkplaceBuffStatus buff, float? days) => AddOrUpdate(buff, days);
    public void RemoveWorkplaceBuff(string buffId) => Remove(buffId);

    protected override void OnLoaded()
    {
        workplaces.OnEntityRegistered += ApplyBuffs;

        foreach (var workplace in workplaces.Entities)
        {
            ApplyBuffs(workplace);
        }
    }

    protected override void Apply(WorkplaceBuffStatus buff)
    {
        foreach (var workplace in GetWorkplaces(buff.Target))
        {
            workplace.GetComponent<WorkplaceWorkerBonusComponent>().AddOrUpdateBonus(buff);
        }
    }

    protected override void RemoveBuff(WorkplaceBuffStatus buff)
    {
        foreach (var workplace in GetWorkplaces(buff.Target))
        {
            workplace.GetComponent<WorkplaceWorkerBonusComponent>().RemoveBonus(buff.Id);
        }
    }

    void ApplyBuffs(WorkplaceTrackerComponent workplace)
    {
        foreach (var buff in Buffs.Values)
        {
            if (WorkplaceHelper.MatchTemplates(buff.Target)(workplace.Workplace))
            {
                workplace.GetComponent<WorkplaceWorkerBonusComponent>().AddOrUpdateBonus(buff);
            }
        }
    }

    IEnumerable<WorkplaceTrackerComponent> GetWorkplaces(WorkplaceBuffTarget target)
    {
        if (target.TemplateNames.Length == 0 && target.TemplateNamePrefixes.Length == 0)
        {
            foreach (var workplace in workplaces.Entities)
            {
                yield return workplace;
            }

            yield break;
        }

        foreach (var workplace in findEntityHelper.FindEntitiesByTemplates<WorkplaceTrackerComponent>(target.TemplateNames, target.TemplateNamePrefixes))
        {
            yield return workplace;
        }
    }
}
