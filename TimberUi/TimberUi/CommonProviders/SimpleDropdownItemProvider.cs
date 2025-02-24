namespace TimberUi.CommonProviders;

public class SimpleDropdownItemProvider(IReadOnlyList<string> items, string? defaultValue) : IDropdownProvider
{
    public IReadOnlyList<string> Items { get; } = items;
    string value = defaultValue ?? items.First();

    public string GetValue()
    {
        return value;
    }

    public void SetValue(string value)
    {
        this.value = value;
    }

}