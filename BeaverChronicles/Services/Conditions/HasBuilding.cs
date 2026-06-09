namespace BeaverChronicles.Services.Conditions;

public record HasBuildingData
{
    public static readonly HasBuildingData Default = new();

    public FrozenSet<string> TemplateNames { get; init; } = [];
    public ImmutableArray<string> TemplateNamePrefixes { get; init; } = [];

    public MinMaxInt Amount { get; init; } = MinMax.Min1;
    public bool IncludeUnderConstruction { get; init; }
    public ImmutableArray<SerializableBoundsInts> Areas { get; init; } = [];
    public AreaCondition AreaCondition { get; init; } = AreaCondition.Intersects;
}

[MultiBind(typeof(IConditionEvaluator))]
public class HasBuilding(
    DefaultEntityTracker<BlockObjectBound> blockObjects
) : ConditionEvaluatorBase<HasBuildingData>
{
    public override string ForType => nameof(HasBuilding);

    protected override bool Evaluate(HasBuildingData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        p ??= HasBuildingData.Default;
        return p.Amount.Evaluate(Count());

        IEnumerable<bool> Count()
        {
            var mustFinish = !p.IncludeUnderConstruction;
            var templates = p.TemplateNames;
            var templatePrefixes = p.TemplateNamePrefixes;

            var hasAreas = p.Areas.Length > 0;
            var areas = hasAreas ? p.Areas.Select(a => (BoundsInt)a).ToArray() : null;
            var areaCond = p.AreaCondition;

            foreach (var bound in blockObjects.Entities)
            {
                if (mustFinish && !bound.BlockObject.IsFinished) { continue; }

                if (templates.Count > 0 || templatePrefixes.Length > 0)
                {
                    var templateName = bound.GetTemplateName();

                    if (!templates.EmptyOrContains(templateName)) { continue; }
                    if (!templatePrefixes.EmptyOrAny(p => templateName.StartsWith(p))) { continue; }
                }

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
