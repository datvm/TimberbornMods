namespace Omnibar.Services.Omnibar.Providers;

public class OmnibarCommandProvider(
    IEnumerable<IOmnibarProvider> providers
) : IOmnibarProvider, ILoadableSingleton
{

    OmnibarCommandItem[] commands = [];
    string[] commandTexts = [];

    public void Load()
    {
        commands = [..providers
            .Where(q => q is IOmnibarCommandProvider)
            .Select(q => new OmnibarCommandItem((IOmnibarCommandProvider)q))
            .OrderBy(q => q.Title)];

        commandTexts = [.. commands.Select(q => q.Title)];
    }

    public IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter)
    {
        // Ignore if already enter a full command
        if (filter.Length > 0 
            && (!filter.IsCommand() 
                || commandTexts.Any(q => filter.StartsWith(q, StringComparison.OrdinalIgnoreCase))))
        {
            return [];
        }

        if (filter.Length == 0)
        {
            return [..commands
                .Select((q, i) => new OmnibarFilteredItem(
                    q,
                    new([], commands.Length - i)))];
        }

        return [..commands
            .Select(q => new OmnibarFilteredItem(
                q, 
                // Special: still show up non-matching commands but at the bottom
                OmnibarUtils.MatchText(filter, q.Title) ?? new([], int.MinValue)))];
    }
}

public class OmnibarCommandItem(IOmnibarCommandProvider provider) : IInplaceExecutionOmnibarItem
{
    public string Title { get; } = provider.Command;
    public IOmnibarDescriptor? Description { get; } = new SimpleLabelDescriptor(provider.CommandDesc);

    public void Execute(OmnibarBox box)
    {
        box.SetText(provider.Command + (provider.Command.EndsWith(" ") ? "" : " "));
    }

    public void Execute() { throw new NotImplementedException(); }

    public bool SetIcon(Image image)
    {
        image.image = provider.Icon;
        return true;
    }
}