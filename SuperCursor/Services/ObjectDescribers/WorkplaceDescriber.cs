
namespace SuperCursor.Services.ObjectDescribers;

public class WorkplaceDescriber(ILoc t) : BaseObjectDescriber<Workplace>
{
    public static readonly string BeaverWorkerType = "Beaver";
    public static readonly string BotWorkerType = "Bot";

    protected override void DescribeComponent(StringBuilder builder, Workplace component)
    {
        builder.Append(t.T("Work.Workplace.DisplayName").Bold());

        var priority = component.GetComponentFast<WorkplacePriority>();
        builder.AppendLine(": " + (priority is null ? "" : priority.Priority.ToTooltipString()));

        var workerType = component.GetComponentFast<WorkplaceWorkerType>();
        var workerTypeText = workerType is null ? null :
            (workerType.WorkerType == BeaverWorkerType
            ? t.T("Beaver.PluralDisplayName") : t.T("Bot.PluralDisplayName"));
        builder.AppendLine($"  {t.T("Work.CurrentWorkers", component.NumberOfAssignedWorkers, component.DesiredWorkers)} {workerTypeText}");

        if (component.DesiredWorkers != component.MaxWorkers)
        {
            builder.AppendLine($"  {t.T("Work.MaximumWorkers", component.MaxWorkers)}");
        }
    }

}
