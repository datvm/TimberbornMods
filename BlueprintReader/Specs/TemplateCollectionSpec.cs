namespace BlueprintReader.Specs;

public record TemplateCollectionSpec(
    string CollectionId,
    ImmutableArray<string> Blueprints
) : IIdBlueprintSpec, ICollectionSpec
{
    public string Id => CollectionId;
    public ImmutableArray<string> Collection => Blueprints;
}
