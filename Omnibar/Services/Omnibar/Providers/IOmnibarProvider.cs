namespace Omnibar.Services.Omnibar.Providers;

public interface IOmnibarProvider
{

    IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter);

}

public interface IOmnibarCommandProvider : IOmnibarProvider
{
    string Command { get; }
    string CommandDesc { get; }
    Texture2D Icon { get; }
}

public readonly record struct OmnibarFilteredItem(IOmnibarItem Item, FuzzyMatchResult Match);
public readonly record struct FuzzyMatchResult(int[] Positions, int Score);