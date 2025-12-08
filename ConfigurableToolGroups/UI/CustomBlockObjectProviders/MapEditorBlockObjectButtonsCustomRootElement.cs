

namespace ConfigurableToolGroups.UI.CustomBlockObjectProviders;

public class MapEditorBlockObjectButtonsCustomRootElement(
    ModdableToolGroupSpecService specs,
    BlockObjectToolButtonFactory boBtnFac, ToolGroupButtonFactory grpFac, ILoc t, ModdableToolGroupButtonService moddableToolGroupButtonService, ToolGroupService toolGroupService
) : CustomBlockObjectButtons(boBtnFac, grpFac, t, moddableToolGroupButtonService, toolGroupService)
{
    public override string Id { get; } = nameof(MapEditorBlockObjectButtons);

    protected override IEnumerable<ToolGroupInfo> GetRootGroups() => specs.RootToolGroup
        .OrderedChildren
        .OfType<ToolGroupInfo>();
}
