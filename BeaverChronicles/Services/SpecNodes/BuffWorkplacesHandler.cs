namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class BuffWorkplacesHandler(
    WorkplaceHelper workplaceHelper
) : NodeHandlerBase<BuffWorkplacesData>
{
    public override string ForType => "BuffWorkplaces";

    protected override string? InternalHandleNode(BuffWorkplacesData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var days = controller.FormatTextFloat(data.Days);

        if (days <= 0)
        {
            workplaceHelper.RemoveWorkplaceBonus(data.BuffId);
        }
        else
        {
            workplaceHelper.AddOrUpdateWorkplaceBonus(new(
                data.BuffId,
                WorkplaceHelper.MatchTemplates(data),
                [.. controller.FormatBonuses(data.Bonuses)],
                controller.FormatTextLoc(data.TitleLoc),
                controller.FormatTextLoc(data.DescLoc)
            ), days);
        }

        return node.NextNodeId;
    }
}
