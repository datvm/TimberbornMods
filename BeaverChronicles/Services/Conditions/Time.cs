namespace BeaverChronicles.Services.Conditions;

public record TimeData
{

}

public class Time : ConditionEvaluatorBase<TimeData>
{
    public override string ForType => "Time";

    protected override bool Evaluate(TimeData? p, ConditionItem c, SpecChronicleEvent ev, ChronicleEventNodeSpec node, ConditionData conditionData)
    {
        throw new NotImplementedException();
    }
}
