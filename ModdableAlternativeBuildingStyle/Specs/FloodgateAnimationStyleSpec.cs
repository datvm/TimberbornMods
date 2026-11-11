namespace ModdableAlternativeBuildingStyle.Specs;

// Gate name the style's model actually uses. Absent on Metal and Original.
public record FloodgateAnimationStyleSpec : ComponentSpec
{
    [Serialize]
    public string GateName { get; init; } = "";
}
