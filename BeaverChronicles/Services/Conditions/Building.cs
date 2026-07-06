namespace BeaverChronicles.Services.Conditions;

public record BuildingConditionData
{
    public ImmutableArray<string> EntityIds { get; init; } = [];
    public ConditionType ConditionType { get; init; } = ConditionType.Any;
    public FrozenSet<string> TemplateNames { get; init; } = [];
    public ImmutableArray<string> TemplateNamePrefixes { get; init; } = [];
    public bool? IsFinished { get; init; }
    public bool ConsiderFreeBuildings { get; init; }
    public ImmutableArray<SerializableBoundsInts> Areas { get; init; } = [];
    public AreaCondition AreaCondition { get; init; } = AreaCondition.Intersects;
}

[MultiBind(typeof(IConditionEvaluator))]
public class BuildingCondition : ConditionEvaluatorBase<BuildingConditionData>
{
    public override string ForType => "Building";

    protected override bool Evaluate(BuildingConditionData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        p ??= new();

        var foundBuildings = ev.Controller.GetEntities<BlockObject>(p.EntityIds.Select(FormatEntityId));
        if (!p.ConsiderFreeBuildings)
        {
            foundBuildings = foundBuildings.Where(ShouldConsider);
        }

        var buildings = foundBuildings.ToArray();
        if (buildings.Length == 0)
        {
            this.LogVerbose(node, () => "No buildings found. Evaluated to false.");
            return false;
        }

        return p.ConditionType.Evaluate(buildings, Matches);

        string FormatEntityId(string entityId) => ev.Controller.FormatText(entityId);

        bool ShouldConsider(BlockObject blockObject)
        {
            var templateName = blockObject.GetTemplateName();            
            var buildingSpec = blockObject.GetComponent<BuildingSpec>();

            return buildingSpec is not null && !buildingSpec.PlaceFinished;
        }

        bool Matches(BlockObject blockObject)
        {
            var templateName = blockObject.GetTemplateName();
            string log = $"- Building {templateName}: ";

            if (p.TemplateNames.Count > 0 && !p.TemplateNames.Contains(templateName))
            {
                this.LogVerbose(node, () => $"{log}TemplateName does not match -> False");
                return false;
            }

            if (p.TemplateNamePrefixes.Length > 0 && !p.TemplateNamePrefixes.Any(prefix => templateName.StartsWith(prefix)))
            {
                this.LogVerbose(node, () => $"{log}TemplateNamePrefixes does not match -> False");
                return false;
            }

            if (p.IsFinished is { } isFinished && blockObject.IsFinished != isFinished)
            {
                this.LogVerbose(node, () => $"{log}IsFinished={isFinished} does not match -> False");
                return false;
            }

            if (p.Areas.Length > 0 && !p.Areas.Any(MatchesArea))
            {
                this.LogVerbose(node, () => $"{log}Areas do not match -> False");
                return false;
            }

            this.LogVerbose(node, () => $"{log}Matches -> True");
            return true;

            bool MatchesArea(SerializableBoundsInts area)
            {
                var bounds = (BoundsInt)area;
                return p.AreaCondition switch
                {
                    AreaCondition.Intersects => BlockObjectHelper.IsIntersectingArea(blockObject, bounds),
                    AreaCondition.Contains => BlockObjectHelper.IsInsideArea(blockObject, bounds),
                    _ => throw new InvalidOperationException($"Unknown area condition: {p.AreaCondition}."),
                };
            }
        }
    }
}
