namespace ConfigurableToolGroups.Specs;

public record ParentToolGroupSpec : ComponentSpec
{
    [Serialize]
    public ImmutableArray<string> ParentIds { get; init; } = [];
}
