namespace BeaverChronicles.Specs.NodeData;

public class GiveData
{
    public GiveDataItem[] Items { get; init; } = [];
}

public class GiveDataItem
{
    /// <summary>
    /// Possible values: Science, AdultBeaver, ChildBeaver, Bot, ID of Good
    /// If none matches, it will be ignored
    /// </summary>
    public string Id { get; init; } = null!;
    public string Amount { get; init; } = null!;
}
