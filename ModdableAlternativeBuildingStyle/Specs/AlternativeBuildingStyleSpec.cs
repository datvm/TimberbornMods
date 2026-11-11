namespace ModdableAlternativeBuildingStyle.Specs;

public record AlternativeBuildingStyleSpec : ComponentSpec
{
    [Serialize]
    public ImmutableArray<AlternativeBuildingStyle> Styles { get; init; } = [];
}

public record AlternativeBuildingStyle
{

    [Serialize]
    public string Name { get; init; } = null!;

    [Serialize]
    public string BlueprintPath { get; init; } = null!;

}