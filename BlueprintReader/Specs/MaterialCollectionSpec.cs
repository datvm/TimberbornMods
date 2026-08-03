namespace BlueprintReader.Specs;

public record MaterialCollectionSpec(
    string CollectionId,
    ImmutableArray<string> Materials
) : IIdBlueprintSpec, ICollectionSpec
{
    public string Id => CollectionId;
    public ImmutableArray<string> Collection => Materials;
}
