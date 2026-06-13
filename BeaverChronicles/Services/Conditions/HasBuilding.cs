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
    FindEntityHelper findEntityHelper
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
            var areas = p.Areas.Select(a => (BoundsInt)a).ToArray();
            foreach (var bound in findEntityHelper.FindBuildings(p.TemplateNames, p.TemplateNamePrefixes, areas, p.AreaCondition))
            {
                if (mustFinish && !bound.BlockObject.IsFinished) { continue; }

                yield return true;
            }
        }
    }

}
