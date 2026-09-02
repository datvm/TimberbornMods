namespace BeaverChronicles.Specs.NodeData;

public record WorkplaceTargetData
{
    public FrozenSet<string> TemplateNames { get; init; } = FrozenSet<string>.Empty;
    public ImmutableArray<string> TemplateNamePrefixes { get; init; } = [];
}
