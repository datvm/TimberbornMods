namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class BuffCharacterNeedsHandler(CharacterStatusHelper characterStatusHelper) : NodeHandlerBase<BuffCharacterNeedsData>
{
    public override string ForType => "BuffCharacterNeeds";

    protected override string? InternalHandleNode(BuffCharacterNeedsData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var ids = GetIds(data, controller);
        float? amount = data.Amount is null ? null : controller.FormatTextFloat(data.Amount);

        if (data.Permanent)
        {
            characterStatusHelper.PermanentNeedBoost.UnionWith(ids);
        }

        characterStatusHelper.BoostAllBeaversNeed(ids, amount);
        return node.NextNodeId;
    }

    static string[] GetIds(BuffCharacterNeedsData data, SpecChronicleEventController controller)
    {
        List<string> ids = [];

        if (data.Id is not null)
        {
            ids.Add(controller.FormatText(data.Id));
        }

        ids.AddRange(data.Ids.Select(controller.FormatText).Where(id => id is not null)!);
        return [.. ids];
    }
}
