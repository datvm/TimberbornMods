namespace BeaverChronicles.Services.Conditions;

public abstract class ConditionEvaluatorBase<TParam> : IConditionEvaluator
     where TParam : class
{
    public abstract string ForType { get; }

    public bool Evaluate(ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
        => Evaluate(c.GetParameters<TParam>(), c, ev, node, conditionData);

    protected abstract bool Evaluate(TParam? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData);

    public static InvalidDataException ThrowMissingData(string type)
        => new("No parameters provided for condition of type " + type);

}
