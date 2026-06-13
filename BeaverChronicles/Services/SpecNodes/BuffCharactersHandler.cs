namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class BuffCharactersHandler(CharacterEntityBuffService characterBuffService) : NodeHandlerBase<BuffCharactersData>
{
    public override string ForType => "BuffCharacters";

    protected override string? InternalHandleNode(BuffCharactersData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var days = controller.FormatTextFloat(data.Days);

        if (data.Category != EntityBuffCategory.Permanent && days <= 0)
        {
            characterBuffService.RemoveCharacterBuff(data.BuffId);
        }
        else
        {
            characterBuffService.AddOrUpdateCharacterBuff(new()
            {
                Id = data.BuffId,
                CharacterType = data.CharacterTypes,
                Effects = [.. controller.FormatBonuses(data.Bonuses)],
                Title = controller.FormatTextLoc(data.TitleLoc),
                Description = controller.FormatTextLoc(data.DescLoc),
                Category = data.Category,
            }, days);
        }

        return node.NextNodeId;
    }
}
