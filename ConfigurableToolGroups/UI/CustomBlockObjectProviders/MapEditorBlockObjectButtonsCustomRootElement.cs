namespace ConfigurableToolGroups.UI.CustomBlockObjectProviders;

public class MapEditorBlockObjectButtonsCustomRootElement(
    ModdableToolGroupSpecService specs,
    ModdableToolGroupButtonFactory grpButtonFac,
    BlockObjectToolButtonFactory boBtnFac,
    BottomBarButtonLookupService lookupService
) : CustomBlockObjectButtons(grpButtonFac, boBtnFac, lookupService)
{
    public override string Id { get; } = nameof(MapEditorBlockObjectButtons);

    protected override IEnumerable<BlockObjectToolGroupInfo> GetRootGroups() => specs.RootToolGroup
        .OrderedChildren
        .OfType<BlockObjectToolGroupInfo>();
}
