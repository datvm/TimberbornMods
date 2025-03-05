namespace ScientificProjects.Specs;

public record ScientificProjectGroupSpec : ComponentSpec
{
    [Serialize]
    public string Id { get; init; } = null!;

    [Serialize]
    public string NameKey { get; init; } = null!;
    public string DisplayName { get; internal set; } = null!;

    [Serialize]
    public string DescKey { get; init; } = null!;
    public string Description { get; internal set; } = null!;

    [Serialize(isOptional: true)]
    public int Order { get; init; }

    public override string ToString() => $"Group {Id}: {NameKey}";
}
