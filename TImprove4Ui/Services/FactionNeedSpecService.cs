namespace TImprove4Ui.Services;

[BindSingleton]
public class FactionNeedSpecService(
    FactionNeedService factionNeedService,
    NeedGroupSpecService needGroupService
) : ILoadableSingleton
{

    public FrozenDictionary<string, NeedGroupSpec> NeedGroupsByIds { get; private set; } = FrozenDictionary<string, NeedGroupSpec>.Empty;
    public FrozenDictionary<string, NeedSpec> NeedsByIds { get; private set; } = FrozenDictionary<string, NeedSpec>.Empty;


    public void Load()
    {
        NeedGroupsByIds = needGroupService.NeedGroups
            .ToFrozenDictionary(q => q.Id);
        NeedsByIds = factionNeedService.Needs
            .ToFrozenDictionary(q => q.Id);
    }
}
