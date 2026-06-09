namespace BeaverChronicles.Specs.NodeData;

public record WorkplaceTargetData
{
    public FrozenSet<string> TemplateNames { get; init; } = [];
    public ImmutableArray<string> TemplateNamePrefixes { get; init; } = [];
}
