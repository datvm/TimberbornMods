namespace BeaverChronicles.Services.Conditions;

public enum  AreaCondition
{
    Intersects,
    Contains
}

public record HasBuildingData
{
    public static readonly HasBuildingData Default = new();

    public FrozenSet<string> TemplateNames { get; init; } = [];
    public MinMaxInt Amount { get; init; } = MinMax.Min1;
    public bool IncludeUnderConstruction { get; init; }
    public ImmutableArray<SerializableInts> Areas { get; init; } = [];
    public AreaCondition AreaCondition { get; init; }
}

[MultiBind(typeof(IConditionEvaluator))]
public class HasBuildingEvaluator(
    DefaultEntityTracker<BlockObject> blockObjects
) : IConditionEvaluator
{
    public ConditionItemType ForType => ConditionItemType.HasBuildings;

    public bool Evaluate(ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        var data = c.GetParameters<HasBuildingData>() ?? HasBuildingData.Default;

        return data.Amount.Evaluate(Count());

        IEnumerable<bool> Count()
        {
            var mustFinish = !data.IncludeUnderConstruction;
            var templates = data.TemplateNames;
            
            var areas = data.Areas.Select(a => (BoundsInt)a);
            var hasAreas = areas.Length > 0;

            foreach (var bo in blockObjects.Entities)
            {
                if (mustFinish && !bo.IsFinished) { continue; }
                if (!templates.EmptyOrContains(bo.GetTemplateName())) { continue; }

                if (hasAreas)
                {
                    var buildingBounds = bo.GetBounds();

                    if (!areas.FastAny(a => )) { continue; }
                }

                yield return true;
            }
        }
    }

}
