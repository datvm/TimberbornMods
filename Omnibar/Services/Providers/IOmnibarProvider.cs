namespace Omnibar.Services.Providers;

public interface IOmnibarProvider
{

    IReadOnlyList<IOmnibarItem> ProvideItems(string? filter);

}
