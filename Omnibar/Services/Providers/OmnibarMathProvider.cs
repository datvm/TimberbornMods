using UnityEngine.UIElements;

namespace Omnibar.Services.Providers;

public class OmnibarMathProvider : IOmnibarProvider
{
    const int MaxQueueLength = 10;

    readonly Queue<OmnibarFilteredItem> history = [];
    int index = 0;

    readonly Jace.CalculationEngine engine = new();

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
            result = engine.Formula(exp)
                .Result(Jace.DataType.FloatingPoint)
                .Build()
                .DynamicInvoke().ToString();
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
    public string Title { get; } = result ?? "?";
    public IOmnibarDescriptor? Description { get; } = new SimpleLabelDescriptor(expression);

    public void Execute()
    {
        if (Title != "?")
        {
            provider.AppendHistory(this);
        }
    }

    public bool SetIcon(Image image) { return false; }
}
