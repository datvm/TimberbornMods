
namespace BeaverChronicles.Services;

[BindSingleton]
public class ChronicleEventConditionService(
    IEnumerable<IConditionEvaluator> conditionEvaluators
) : ILoadableSingleton
{

    readonly FrozenDictionary<ConditionItemType, IConditionEvaluator> evaluators
        = conditionEvaluators.ToFrozenDictionary(e => e.ForType);

    public void Load()
    {
        foreach (var c in TimberUiUtils.GetSortedEnumValues<ConditionItemType>())
        {
            if (!evaluators.ContainsKey(c))
            {
                throw new InvalidOperationException($"No condition evaluator found for type {c}");
            }
        }
    }

    public bool Evaluate(SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        var cs = conditionData.Conditions;
        if (cs.Length == 0) { return true; }

        return conditionData.ConditionType.Evaluate(cs, 
            c => evaluators[c.Type].Evaluate(c, ev, node, conditionData));
    }

}
