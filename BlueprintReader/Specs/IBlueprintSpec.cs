namespace BlueprintReader.Specs;

public interface IBlueprintSpec;

public interface IIdBlueprintSpec : IBlueprintSpec
{
    string Id { get; }
}

public interface ICollectionSpec : IIdBlueprintSpec
{
    ImmutableArray<string> Collection { get; }
}
