namespace QuickBar.Models;

public interface IQuickBarItemProvider
{
    ImmutableHashSet<Type> SupportedType { get; }

    bool TryCreateItem(IOmnibarItem omnibarItem, [NotNullWhen(true)] out IQuickBarItem? quickbarItem);

    string? Serialize(IQuickBarItem item);
    IQuickBarItem? Deserialize(string data);
}
