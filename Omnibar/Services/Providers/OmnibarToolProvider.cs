
namespace Omnibar.Services.Providers;

public class OmnibarToolProvider(
    ToolButtonService toolButtonService,
    DevModeManager devModeManager,
    ILoc t
) : IOmnibarProvider, ILoadableSingleton
{

    ImmutableArray<OmnibarToolItem> items;

    public void Load()
    {
        items = [..toolButtonService.ToolButtons
            .Select(q => new OmnibarToolItem(q, t))];
    }

    public IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter)
    {
        var devModeOn = devModeManager.Enabled;

        return [..items
            .Select(q => (q, OmnibarUtils.MatchText(filter, q.Title)))
            .Where(q =>
                q.Item2.HasValue
                && (devModeOn || !q.q.ToolButton.DevModeTool))
            .Select(q => new OmnibarFilteredItem(q.q, q.Item2!.Value))];
    }

}
