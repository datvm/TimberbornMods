namespace ModdableAlternativeBuildingStyle.Specs;

// Turret and barrel the style's model actually uses. Absent on Original.
public record FireworkLauncherAnimationStyleSpec : ComponentSpec
{
    [Serialize]
    public string Turret { get; init; } = "";

    [Serialize]
    public string Barrel { get; init; } = "";
}
