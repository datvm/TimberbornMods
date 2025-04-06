namespace ScientificProjects.Management;

public class ModProjectCostProvider(
    BeaverPopulation pops,
    EntityManager entities
) : IProjectCostProvider
{
    private const string WorkEffUpgrade2 = "WorkEffUpgrade2";
    private const string BuilderCarryUpgrade = "CarryBuilderUpgrade";
    static readonly ImmutableArray<string> Ids = [
        WorkEffUpgrade2,
        BuilderCarryUpgrade,
    ];
    public IEnumerable<string> CanCalculateCostForIds => Ids;

    public int CalculateCost(ScientificProjectInfo project)
    {
        var spec = project.Spec;
        return spec.Id switch
        {
            // 3 per level + 1 per 20 adult beavers (rounded up)
            WorkEffUpgrade2 => this.LevelOr0F(project, l => spec.ScienceCost * l + (spec.Parameters[1] * CountAdultBeavers() / spec.Parameters[2])),
            BuilderCarryUpgrade => this.LevelOr0F(project, l => spec.ScienceCost * l * CountBuilders()),

            _ => throw spec.ThrowNotSupportedEx(),
        };
    }


    int CountAdultBeavers() => pops.NumberOfAdults;
    int CountBuilders() => CountEmployees<DistrictCenter>() + CountEmployees<BuilderHubSpec>();

    int CountEmployees<T>() where T : BaseComponent
    {
        var building = entities.Get<T>();
        var total = 0;
        foreach (var b in building)
        {
            var wp = b.GetComponentFast<Workplace>();
            if (wp is null) { continue; }

            total += wp.NumberOfAssignedWorkers;
        }

        return total;
    }

}
