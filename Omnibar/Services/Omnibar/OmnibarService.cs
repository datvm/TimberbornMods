
namespace Omnibar.Services.Omnibar;

public class OmnibarService(
    IEnumerable<IOmnibarProvider> providers,
    OmnibarCommandProvider commandProvider,
    IEnumerable<IOmnibarHotkeyProvider> hotkeyProviders
)
{

    public List<OmnibarBoxItem> GetItems(string request)
    {
        var lowerCase = request.TrimStart().ToLower();

        IEnumerable<OmnibarFilteredItem> commands = commandProvider.ProvideItems(lowerCase);

        if (!commands.Any())
        {
            commands = providers
                .SelectMany(q => q.ProvideItems(lowerCase));
        }

        return [..commands
            .OrderByDescending(q => q.Match.Score)
            .ThenBy(q => q.Item.Title)
            .Select(item => {
                var hotkeys = hotkeyProviders
                    .Select(q => q.GetAction(item.Item))
                    .Where(q => q is not null);

                return new OmnibarBoxItem(item, [..hotkeys!]);
            })
        ];
    }

}
