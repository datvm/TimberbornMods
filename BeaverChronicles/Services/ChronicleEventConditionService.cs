namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleEventConditionService(IEnumerable<IConditionEvaluator> conditionEvaluators)
{

    readonly FrozenDictionary<string, IConditionEvaluator> evaluators
        = conditionEvaluators.ToFrozenDictionary(e => e.ForType);
    public bool HasEvaluator(string conditionType) => evaluators.ContainsKey(conditionType);

    public bool Evaluate(SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        var cs = conditionData.Conditions;
        if (cs.Length == 0) { return true; }

        return conditionData.ConditionType.Evaluate(cs,
            c =>
            {
                if (!evaluators.TryGetValue(c.Type, out var evaluator))
                {
                    throw new InvalidOperationException($"No condition evaluator found for type {c.Type}");
                }

                return evaluator.Evaluate(c, ev, node, conditionData);
            });
    }

}
