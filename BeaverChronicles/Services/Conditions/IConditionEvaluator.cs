namespace BeaverChronicles.Services.Conditions;

public interface IConditionEvaluator
{
    ConditionItemType ForType { get; }

    bool Evaluate(ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData);
}
