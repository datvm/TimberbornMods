namespace ScienceShop.Specs;

public record ConsumeScienceManufactorySpec : ComponentSpec;

public record ConsumeScienceRecipeSpec : ComponentSpec
{
    [Serialize]
    public int ScienceCost { get; init; }
}
