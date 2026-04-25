
namespace ConfigurableToolGroups.UI.CustomBlockObjectProviders;

public class GameBlockObjectButtonsCustomRootElement
(
    ModdableToolGroupSpecService specs,
    ModdableToolGroupButtonFactory grpButtonFac,
    BlockObjectToolButtonFactory boBtnFac,
    BottomBarButtonLookupService lookupService
) : CustomBlockObjectButtons(grpButtonFac, boBtnFac, lookupService)
{
    public override string Id { get; } = nameof(GameBlockObjectButtons);

    protected override IEnumerable<BlockObjectToolGroupInfo> GetRootGroups() => specs.RootToolGroup
        .OrderedChildren
        .OfType<BlockObjectToolGroupInfo>();
}
