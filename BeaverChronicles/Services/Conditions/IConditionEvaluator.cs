namespace BeaverChronicles.Services.Conditions;

public interface IConditionEvaluator
{
    string ForType { get; }

    bool Evaluate(ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData);
}
