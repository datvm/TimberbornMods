namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class CharacterStatusHandler(
    CharacterStatusHelper statusHelper
) : NodeHandlerBase<CharacterStatusData>
{
    public override string ForType => "CharacterStatus";

    protected override string? InternalHandleNode(CharacterStatusData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var entityIds = data.EntityIds
            .Select(controller.FormatText)
            .Where(id => id is not null)
            .Select(id => id!);
        foreach (var character in controller.GetEntities<Character>(entityIds))
        {
            if (controller.FormatText(data.RemoveNeed) is { } removeNeed)
            {
                statusHelper.RemoveNeed(character, removeNeed);
            }

            if (controller.FormatText(data.InflictNeed) is { } inflictNeed)
            {
                statusHelper.InflictNeed(character, inflictNeed);
            }
        }

        return node.NextNodeId;
    }
}
