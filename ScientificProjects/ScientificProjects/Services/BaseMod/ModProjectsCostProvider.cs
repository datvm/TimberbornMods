namespace ScientificProjects.Services.BaseMod;

public class ModProjectsCostProvider(
    CharacterTracker characters
) : IProjectCostProvider
{
    private const string WorkEffUpgrade2 = "WorkEffUpgrade2";
    private const string BuilderCarryUpgrade = "CarryBuilderUpgrade";
    static readonly ImmutableArray<string> Ids = [
        WorkEffUpgrade2,
        BuilderCarryUpgrade,
    ];
    public IEnumerable<string> CanCalculateCostForIds => Ids;

    public int CalculateCost(ScientificProjectSpec spec, int level)
    {
        return spec.Id switch
        {
            // 3 per level + 1 per 20 adult beavers (rounded up)
            WorkEffUpgrade2 => this.LevelOr0F(spec, level, l => spec.ScienceCost * l + spec.Parameters[1] * CountAdultBeavers() / spec.Parameters[2]),
            BuilderCarryUpgrade => this.LevelOr0F(spec, level, l => spec.ScienceCost * l * CountBuilders()),

            _ => throw spec.ThrowNotSupportedEx(),
        };
    }

    int CountAdultBeavers() => characters.Adults.Count;
    int CountBuilders() => characters.Workers.Count(q =>
    {
        var workplace = q.Worker ? q.Worker.Workplace : null;
        return workplace && (workplace.GetComponentFast<DistrictCenter>() || workplace.GetComponentFast<BuilderHubSpec>());
    });


}
