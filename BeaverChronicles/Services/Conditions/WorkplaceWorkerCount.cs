namespace BeaverChronicles.Services.Conditions;

public record WorkplaceWorkerCountData : WorkplaceTargetData
{
    public MinMaxInt Amount { get; init; } = MinMax.Min1;
}

[MultiBind(typeof(IConditionEvaluator))]
public class WorkplaceWorkerCount(
    WorkplaceHelper workplaceHelper
) : ConditionEvaluatorBase<WorkplaceWorkerCountData>
{
    public override string ForType => nameof(WorkplaceWorkerCount);

    protected override bool Evaluate(WorkplaceWorkerCountData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        p ??= new();

        var count = workplaceHelper.CountAssignedWorkers(WorkplaceHelper.MatchTemplates(p));
        var match = p.Amount.Evaluate(count);

        this.LogVerbose(node, () => $"- Found {count} assigned workers, compare with {p.Amount} -> Evaluated to {match}");

        return match;
    }
}
