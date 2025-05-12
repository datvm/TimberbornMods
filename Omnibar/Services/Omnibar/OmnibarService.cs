
namespace Omnibar.Services.Omnibar;

public class OmnibarService(
    IEnumerable<IOmnibarProvider> providers,
    OmnibarCommandProvider commandProvider
)
{

    public List<OmnibarFilteredItem> GetItems(string request)
    {
        var commands = commandProvider.ProvideItems(request);
        
        return commands.Count > 0 ? [..commands] : [..providers
            .SelectMany(q => q.ProvideItems(request))
            .OrderByDescending(q => q.Match.Score)
            .ThenBy(q => q.Item.Title)
        ];
    }

}
