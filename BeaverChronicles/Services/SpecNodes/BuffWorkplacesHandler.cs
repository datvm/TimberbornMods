namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class BuffWorkplacesHandler(
    WorkplaceEntityBuffService workplaceBuffService
) : NodeHandlerBase<BuffWorkplacesData>
{
    public override string ForType => "BuffWorkplaces";

    protected override string? InternalHandleNode(BuffWorkplacesData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var days = controller.FormatTextFloat(data.Days);

        if (data.Category != EntityBuffCategory.Permanent && days <= 0)
        {
            workplaceBuffService.RemoveWorkplaceBuff(data.BuffId);
        }
        else
        {
            workplaceBuffService.AddOrUpdateWorkplaceBuff(new()
            {
                Id = data.BuffId,
                Target = new()
                {
                    TemplateNames = [.. data.TemplateNames],
                    TemplateNamePrefixes = [.. data.TemplateNamePrefixes],
                },
                Effects = [.. controller.FormatBonuses(data.Bonuses)],
                Title = controller.FormatTextLoc(data.TitleLoc),
                Description = controller.FormatTextLoc(data.DescLoc),
                Category = data.Category,
            }, days);
        }

        return node.NextNodeId;
    }
}
