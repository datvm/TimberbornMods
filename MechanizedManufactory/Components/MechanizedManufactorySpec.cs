namespace MechanizedManufactory.Components;

public record MechanizedManufactorySpec : ComponentSpec
{

    [Serialize]
    public int Workers { get; init; }

}
