namespace Omnibar.Services;

public class OmnibarService(
    IEnumerable<IOmnibarProvider> providers
)
{

    public IReadOnlyList<IOmnibarItem> GetItems(string request)
    {
        return [..providers
            .SelectMany(q => q.ProvideItems(request))
            .OrderBy(q => q.Title.Length)
        ];
    }

}
