namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class BuffCharactersHandler(CharacterStatusHelper characterStatusHelper) : NodeHandlerBase<BuffCharactersData>
{
    public override string ForType => "BuffCharacters";

    protected override string? InternalHandleNode(BuffCharactersData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var days = controller.FormatTextFloat(data.Days);

        if (days <= 0)
        {
            characterStatusHelper.RemoveLimitedTimeStatus(data.BuffId);
        }
        else
        {
            characterStatusHelper.AddOrUpdateLimitedTimeBonus(new(
                data.BuffId,
                data.CharacterTypes,
                [.. controller.FormatBonuses(data.Bonuses)],
                controller.FormatTextLoc(data.TitleLoc),
                controller.FormatTextLoc(data.DescLoc)
            ), days);
        }

        return node.NextNodeId;
    }
}
