
namespace BeaverChronicles.Services.Conditions;

public record HasBuildingData
{
    public static readonly HasBuildingData Default = new();

    public FrozenSet<string> TemplateNames { get; init; } = [];
    public MinMaxInt Amount { get; init; } = MinMax.Min1;
    public bool IncludeUnderConstruction { get; init; }
    public ImmutableArray<SerializableBoundsInts> Areas { get; init; } = [];
    public AreaCondition AreaCondition { get; init; } = AreaCondition.Intersects;
}

[MultiBind(typeof(IConditionEvaluator))]
public class HasBuilding(
    DefaultEntityTracker<BlockObjectBound> blockObjects
) : IConditionEvaluator
{
    public string ForType => nameof(HasBuilding);

    public bool Evaluate(ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        var data = c.GetParameters<HasBuildingData>() ?? HasBuildingData.Default;
        return data.Amount.Evaluate(Count());

        IEnumerable<bool> Count()
        {
            var mustFinish = !data.IncludeUnderConstruction;
            var templates = data.TemplateNames;

            var hasAreas = data.Areas.Length > 0;
            var areas = hasAreas ? data.Areas.Select(a => (BoundsInt)a).ToArray() : null;
            var areaCond = data.AreaCondition;

            foreach (var bound in blockObjects.Entities)
            {
                if (mustFinish && !bound.BlockObject.IsFinished) { continue; }
                if (!templates.EmptyOrContains(bound.GetTemplateName())) { continue; }

                if (hasAreas)
                {
                    var buildingBounds = bound.Bounds;
                    if (!areas.FastAny(a => areaCond.Evaluate(a, buildingBounds))) { continue; }
                }

                yield return true;
            }
        }
    }

}
