namespace MoreBuildingRenovations.Components;

public record PlantsSpeedUpSpec : ComponentSpec
{
    [Serialize]
    public PlantsSpeedUpType AffectingType { get; init; }
}

public enum PlantsSpeedUpType
{
    Crops,
    Trees,
    TreeProducts,
    BushProducts,
}