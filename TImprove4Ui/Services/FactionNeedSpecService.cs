namespace TImprove4Ui.Services;

public class FactionNeedSpecService(
    FactionNeedService factionNeedService,
#if TIMBERU7
    NeedGroupService needGroupService
#else
    NeedGroupSpecService needGroupService
#endif
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
