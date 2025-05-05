namespace Omnibar.Services.Providers;

public class OmnibarToolProvider(
    BottomBarPanel bottomBarPanel
) : IOmnibarProvider, ILoadableSingleton
{

    public void Load()
    {

    }

    public IReadOnlyList<IOmnibarItem> ProvideItems(string? filter)
    {

    }
}
