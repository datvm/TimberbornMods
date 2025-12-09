namespace ConfigurableToolGroups.UI.CustomBlockObjectProviders;

public class MapEditorBlockObjectButtonsCustomRootElement(
    ModdableToolGroupSpecService specs,
    ModdableToolGroupButtonFactory grpButtonFac,
    BlockObjectToolButtonFactory boBtnFac
) : CustomBlockObjectButtons(grpButtonFac, boBtnFac)
{
    public override string Id { get; } = nameof(MapEditorBlockObjectButtons);

    protected override IEnumerable<BlockObjectToolGroupInfo> GetRootGroups() => specs.RootToolGroup
        .OrderedChildren
        .OfType<BlockObjectToolGroupInfo>();
}
