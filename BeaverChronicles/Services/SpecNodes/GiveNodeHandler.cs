namespace BeaverChronicles.Services.SpecNodes;

[MultiBind(typeof(ISpecNodeHandler))]
public class GiveNodeHandler(
    GoodsHelper goodsHelper,
    CharacterSpawnHelper characterSpawnHelper,
    FindEntityHelper findEntityHelper) : NodeHandlerBase<GiveData>
{
    public override string ForType => "Give";

    protected override string? InternalHandleNode(GiveData data, ChronicleEventNodeSpec node, SpecChronicleEventController controller)
    {
        var dcId = controller.FormatText(data.PreferedDistrictCenter);
        var dc = dcId is null ? null : findEntityHelper.FindDistrictCenter(dcId);

        if (data.Items.Length > 0)
        {
            node.LogVerbose(() => $"Giving items: {string.Join(", ", data.Items.Select(i => $"{i.Id} x{i.Amount}"))}.");
            goodsHelper.GiveGoodsAndScience(controller.FormatItems(data.Items), dc);
        }

        if (data.Science is not null)
        {
            var science = controller.FormatTextInt(data.Science);

            if (science != 0)
            {
                node.LogVerbose(() => $"Giving science: {science}.");
                goodsHelper.ModifyScience(science);
            }
        }

        if (data.Spawns.Length > 0)
        {
            SpawnCharacters();
        }

        return node.NextNodeId;

        void SpawnCharacters()
        {
            if (!characterSpawnHelper.FindAnySpawnSpot(out var location))
            {
                Debug.LogWarning("No spawn location found for character spawns, skipping.");
                return;
            }

            foreach (var s in controller.FormatItems(data.Spawns))
            {
                switch (s.GoodId)
                {
                    case nameof(CharacterType.AdultBeaver):
                        node.LogVerbose(() => $"Spawning {s.Amount} Adult Beavers at {location}.");
                        characterSpawnHelper.Spawn(s.Amount, location, CharacterType.AdultBeaver);
                        break;
                    case nameof(CharacterType.ChildBeaver):
                        node.LogVerbose(() => $"Spawning {s.Amount} Child Beavers at {location}.");
                        characterSpawnHelper.Spawn(s.Amount, location, CharacterType.ChildBeaver);
                        break;
                    case nameof(CharacterType.Bot):
                        node.LogVerbose(() => $"Spawning {s.Amount} Bots at {location}.");
                        characterSpawnHelper.Spawn(s.Amount, location, CharacterType.Bot);
                        break;
                    default:
                        Debug.LogWarning($"Unknown spawn type {s.GoodId}");
                        break;
                }
            }
        }
    }
}
