namespace SuperCursor.Services.ObjectDescribers;

public class RuinDescriber(IGoodService goods, ILoc t) : BaseObjectDescriber<Ruin>
{
    
    protected override void DescribeComponent(StringBuilder builder, Ruin component)
    {
        var y = component.Yielder;
        var curr = y.Yield;
        var max = y._initialYield.Amount;

        builder.AppendLine(t.T("LV.SC.Ruin",
            curr.Amount,
            max,
            goods.GetGood(curr.GoodId).DisplayName.Value));
    }

}
