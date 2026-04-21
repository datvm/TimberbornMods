namespace Omnibar.Services.Omnibar.Providers;

public class OmnibarToolProvider(
    ToolButtonService toolButtonService,
    DevModeManager devModeManager,
    ILoc t,
    IContainer container
) : IOmnibarProvider, IPostLoadableSingleton
{

    OmnibarToolItem[] items = [];
    public FrozenDictionary<string, BlockObjectTool> BuildingTools { get; private set; } = null!;
    public IReadOnlyList<OmnibarToolItem> Items => items;

    public void PostLoad()
    {
        items = [.. toolButtonService.ToolButtons.Select(q => new OmnibarToolItem(q, t, container))];

        BuildingTools = items
            .Where(q => q.BuildingSpec is not null && q.ToolButton.Tool is BlockObjectTool)
            .ToFrozenDictionary(q => q.BuildingName!, q => (BlockObjectTool)q.ToolButton.Tool);
    }

    public IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter)
    {
        if (filter.IsCommand()) { return []; }

        var devModeOn = devModeManager.Enabled;

        return [..items
            .Select(q => (q, OmnibarUtils.MatchText(filter, q.Title)))
            .Where(q =>
                q.Item2.HasValue
                && (devModeOn || !q.q.ToolButton.DevModeTool))
            .Select(q => new OmnibarFilteredItem(q.q, q.Item2!.Value))];
    }

}
