namespace BuildingHP.Specs;

public record BuildingMaterialDurabilitySpec : ComponentSpec
{

    [Serialize]
    public string GoodId { get; init; } = null!;

    [Serialize]
    public int Durability { get; init; }

}
