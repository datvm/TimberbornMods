namespace Omnibar.Services.Omnibar.Providers;

public class OmnibarMathProvider(IAssetLoader assets) : IOmnibarProvider, ILoadableSingleton
{
    const int MaxQueueLength = 10;

    public Texture2D Icon { get; private set; } = null!;

    readonly Queue<OmnibarFilteredItem> history = [];
    int index = 0;

    public void Load()
    {
        Icon = assets.Load<Texture2D>("Sprites/Omnibar/calculator");
    }

    public IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter)
    {
        if (filter[0] != OmnibarUtils.MathTrigger) { return []; }

        var result = TryCalculating(filter.Substring(1));
        return [new(result, new([], int.MaxValue)), .. history];
    }

    OmnibarMathResult TryCalculating(string exp)
    {
        string? result;

        try
        {
            result = new Mathos.Parser.MathParser()
                .Parse(exp)
                .ToString();
        }
        catch (Exception)
        {
            result = null;
        }

        return new(result, exp, this);
    }

    public void AppendHistory(OmnibarMathResult omnibarMathResult)
    {
        if (history.Count >= MaxQueueLength)
        {
            history.Dequeue();
        }

        history.Enqueue(new(omnibarMathResult, new([], ++index)));
    }

}

public class OmnibarMathResult(string? result, string expression, OmnibarMathProvider provider) : IOmnibarItem
{
    public string Title { get; } = (result ?? "?").Color(TimberbornTextColor.Solid).Bold().Size(20);
    public IOmnibarDescriptor? Description { get; } = new SimpleLabelDescriptor("= " + expression.Trim());
    public bool CanAddToTodoList { get; } = false;

    public void Execute()
    {
        if (result is not null)
        {
            provider.AppendHistory(this);
        }
    }

    public bool SetIcon(Image image)
    {
        image.image = provider.Icon;
        return true;
    }
}
