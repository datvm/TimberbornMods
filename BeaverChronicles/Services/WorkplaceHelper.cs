namespace BeaverChronicles.Services;

[BindSingleton]
public class WorkplaceHelper(
    WorkplaceTracker workplaces
)
{
    public delegate bool WorkplaceFilter(Workplace workplace);

    public bool HasMinimumWorkers(int expected, WorkplaceFilter workplaceFilter)
    {
        if (expected <= 0) { return true; }

        var counter = 0;

        foreach (var workplace in GetWorkplaces(workplaceFilter))
        {
            counter += workplace.AssignedWorkers.Count;

            if (counter >= expected)
            {
                return true;
            }
        }

        return false;
    }

    public int CountAssignedWorkers(WorkplaceFilter workplaceFilter)
        => GetWorkplaces(workplaceFilter).Sum(workplace => workplace.AssignedWorkers.Count);

    public static WorkplaceFilter MatchTemplates(WorkplaceTargetData target)
        => MatchTemplates([.. target.TemplateNames], [.. target.TemplateNamePrefixes]);

    public static WorkplaceFilter MatchTemplates(WorkplaceBuffTarget target)
        => MatchTemplates(target.TemplateNames, target.TemplateNamePrefixes);

    static WorkplaceFilter MatchTemplates(IReadOnlyCollection<string> templates, IReadOnlyCollection<string> templatePrefixes)
    {
        return workplace =>
        {
            if (templates.Count == 0 && templatePrefixes.Count == 0)
            {
                return true;
            }

            var templateName = workplace.GetTemplateName();
            return templates.EmptyOrContains(templateName)
                && templatePrefixes.EmptyOrAny(prefix => templateName.StartsWith(prefix));
        };
    }

    public static bool IsFarmhouse(Workplace workplace)
    {
        var spec = workplace.GetComponent<PlanterBuildingSpec>();
        return spec is not null && spec.PlantableResourceGroup == "Farmhouse";
    }

    public static bool IsLumberjack(Workplace workplace)
    {
        var spec = workplace.GetComponent<YieldRemovingBuildingSpec>();
        return spec is not null && spec.ResourceGroup == "Cuttable";
    }

    public static bool AllWorkplaces(Workplace _) => true;

    public IEnumerable<Workplace> GetWorkplaces(WorkplaceFilter workplaceFilter)
    {
        foreach (var e in workplaces.Entities)
        {
            var workplace = e.Workplace;
            if (workplaceFilter(workplace))
            {
                yield return workplace;
            }
        }
    }

}
