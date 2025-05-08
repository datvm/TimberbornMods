namespace Omnibar.Services;

public class OmnibarService(
    IEnumerable<IOmnibarProvider> providers
)
{

    public List<OmnibarFilteredItem> GetItems(string request)
    {
        return [..providers
            .SelectMany(q => q.ProvideItems(request))
            .OrderByDescending(q => q.Match.Score)
        ];
    }

}
