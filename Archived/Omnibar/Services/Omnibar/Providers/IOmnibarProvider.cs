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

public readonly record struct OmnibarBoxItem(IOmnibarItem Item, FuzzyMatchResult Match, IReadOnlyList<IOmnibarHotkeyAction> HotkeyActions)
{
    public OmnibarBoxItem(in OmnibarFilteredItem item, IReadOnlyList<IOmnibarHotkeyAction> hotkeyActions) : this(item.Item, item.Match, hotkeyActions) { }

    public static implicit operator OmnibarFilteredItem(OmnibarBoxItem item)
    {
        return new(item.Item, item.Match);
    }
}

public readonly record struct OmnibarFilteredItem(IOmnibarItem Item, FuzzyMatchResult Match);
public readonly record struct FuzzyMatchResult(int[] Positions, int Score)
{
    public static readonly FuzzyMatchResult Empty = new([], 0);
}