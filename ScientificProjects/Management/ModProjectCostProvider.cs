namespace ScientificProjects.Management;

public class ModProjectCostProvider(
    BeaverPopulation pops
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
            WorkEffUpgrade2 => LevelOr0F(project, l => spec.ScienceCost * l + (spec.Parameters[1] * CountAdultBeavers() / spec.Parameters[2])),
            BuilderCarryUpgrade => LevelOr0F(project, l => spec.ScienceCost * l * CountBuilders()),

            _ => throw new NotSupportedException($"Cannot calculate cost for Id {spec.Id} ({spec.DisplayName})"),
        };
    }

    int LevelOr0(ScientificProjectInfo info, Func<int, int> calculate) => info.Level == 0 ? 0 : calculate(info.Level);
    int LevelOr0F(ScientificProjectInfo info, Func<int, float> calculate) => info.Level == 0 ? 0 : (int)MathF.Ceiling(calculate(info.Level));

    int CountAdultBeavers() => pops.NumberOfAdults;

    int CountBuilders() => 0;

}
