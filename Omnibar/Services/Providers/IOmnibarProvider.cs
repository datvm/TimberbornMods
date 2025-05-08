namespace Omnibar.Services.Providers;

public interface IOmnibarProvider
{

    IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter);

}

public readonly record struct OmnibarFilteredItem(IOmnibarItem Item, FuzzyMatchResult Match);
public readonly record struct FuzzyMatchResult(int[] Positions, int Score);