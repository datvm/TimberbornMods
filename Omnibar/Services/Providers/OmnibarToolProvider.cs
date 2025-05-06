
namespace Omnibar.Services.Providers;

public class OmnibarToolProvider(
    ToolButtonService toolButtonService,
    DevModeManager devModeManager
) : IOmnibarProvider, ILoadableSingleton
{

    ImmutableArray<OmnibarToolItem> items;

    public void Load()
    {
        items = [..toolButtonService.ToolButtons
            .Select(q => new OmnibarToolItem(q))];
    }

    public IReadOnlyList<IOmnibarItem> ProvideItems(string filter)
    {
        var devModeOn = devModeManager.Enabled;

        return [..items.Where(q =>
            OmnibarUtils.MatchText(filter, q.Title)
            && (devModeOn || !q.ToolButton.DevModeTool))];
    }

}
