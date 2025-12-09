
namespace ConfigurableToolGroups.UI.CustomBlockObjectProviders;

public class GameBlockObjectButtonsCustomRootElement
(
    ModdableToolGroupSpecService specs,
    ModdableToolGroupButtonFactory grpButtonFac,
    BlockObjectToolButtonFactory boBtnFac
) : CustomBlockObjectButtons(grpButtonFac, boBtnFac)
{
    public override string Id { get; } = nameof(GameBlockObjectButtons);

    protected override IEnumerable<BlockObjectToolGroupInfo> GetRootGroups() => specs.RootToolGroup
        .OrderedChildren
        .OfType<BlockObjectToolGroupInfo>();
}
