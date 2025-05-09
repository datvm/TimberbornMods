namespace Omnibar.Services.Providers;

public class OmnibarToolProvider(
    ToolButtonService toolButtonService,
    DevModeManager devModeManager,
    ILoc t,
    IContainer container
) : IOmnibarProvider, ILoadableSingleton
{

    OmnibarToolItem[] items = [];

    public void Load()
    {
        items = [.. toolButtonService.ToolButtons.Select(q => new OmnibarToolItem(q, t, container))];
    }

    public IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter)
    {
        // Only work for non-special triggers
        if (OmnibarUtils.SpecialTriggers.Contains(filter[0])) { return []; }

        var devModeOn = devModeManager.Enabled;

        return [..items
            .Select(q => (q, OmnibarUtils.MatchText(filter, q.Title)))
            .Where(q =>
                q.Item2.HasValue
                && (devModeOn || !q.q.ToolButton.DevModeTool))
            .Select(q => new OmnibarFilteredItem(q.q, q.Item2!.Value))];
    }

}
