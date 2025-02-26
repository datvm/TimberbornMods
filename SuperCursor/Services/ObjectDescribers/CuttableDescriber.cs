
namespace SuperCursor.Services.ObjectDescribers;

[ServicePriority(1)]
public class CuttableDescriber(DeadNaturalResourceDescriber deadDescriber, ILoc t, IGoodService goods) : BaseObjectDescriber<Cuttable>
{
    const string DayLoc = "Time.DaysShort";

    protected override void DescribeComponent(StringBuilder builder, Cuttable component)
    {
        DescribeDying(builder, component);

        var yield = component.YielderSpec.Yield;
        var growable = component.GetComponentFast<Growable>();
        if (growable is not null)
        {
            DescribeGrowable(builder, growable, yield);
        }

        var gatherable = component.GetComponentFast<Gatherable>();
        if (gatherable is not null)
        {
            DescribeGatherable(builder, gatherable);
        }
    }

    string GetYieldText(GoodAmountSpec yield) 
        => $"  {yield.Amount}x {goods.GetGood(yield.GoodId).PluralDisplayName.Value}: ";

    string GetGrowthText(float progress, float total)
    {
        var current = total * progress;

        return string.Format("{0}/{1} ({2:F0}%)",
            t.T(DayLoc, current.ToString("F1")),
            t.T(DayLoc, total.ToString("F1")),
            progress * 100f
        );
    }

    void DescribeGatherable(StringBuilder builder, Gatherable gatherable)
    {
        var yield = gatherable.YielderSpec.Yield;
        builder.Append(GetYieldText(yield));

        var grower = gatherable.GetComponentFast<GatherableYieldGrower>();
        var gatherTotalTime = gatherable.YieldGrowthTimeInDays;
        var gatherProgress = grower.GrowthProgress;
        builder.AppendLine(GetGrowthText(gatherProgress, gatherTotalTime));
    }

    void DescribeGrowable(StringBuilder builder, Growable growable, GoodAmountSpec yield)
    {
        builder.Append(GetYieldText(yield));

        var growTotalTime = growable.GrowthTimeInDays;
        var growProgress = growable.GrowthProgress;
        builder.AppendLine(GetGrowthText(growProgress, growTotalTime));
    }

    bool DescribeDying(StringBuilder builder, Cuttable cuttable)
    {
        if (cuttable.Yielder.IsYieldRemoved) { return false; }

        var dying = cuttable.GetComponentFast<DyingNaturalResource>();
        if (dying is null) { return false; }

        var progress = dying.GetClosestDyingProgress();
        if (progress.Died)
        {
            builder.AppendLine("  " + deadDescriber.Describe(dying));
        }
        else if (progress.IsDying)
        {
            builder.AppendLine("  " + t.T("NaturalResources.DaysToDie", progress.DaysLeft.ToString("F2")));
        }
        else
        {
            builder.AppendLine(" " + t.T("NaturalResources.Healthy"));
            return false;
        }

        return true;
    }

}
