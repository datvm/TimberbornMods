namespace ConfigurableToolGroups.UI.CustomBlockObjectProviders;

public class MapEditorBlockObjectButtonsCustomRootElement(
    ModdableToolGroupSpecService specs,
    ModdableToolGroupButtonFactory grpButtonFac,
    BlockObjectToolButtonFactory boBtnFac,
    BottomBarButtonLookupService lookupService
) : CustomBlockObjectButtons(grpButtonFac, boBtnFac, lookupService)
{
    static readonly HashSet<string> BlacklistedTemplateNames = ["Path"];

    public override string Id { get; } = nameof(MapEditorBlockObjectButtons);

    protected override IEnumerable<BlockObjectToolGroupInfo> GetRootGroups() => specs.RootToolGroup
        .OrderedChildren
        .OfType<BlockObjectToolGroupInfo>();

    protected override bool IncludePlaceable(PlaceableToolInfo tool)
    {
        if (BlacklistedTemplateNames.Contains(tool.TemplateName)) { return false; }

        return base.IncludePlaceable(tool);
    }
}
