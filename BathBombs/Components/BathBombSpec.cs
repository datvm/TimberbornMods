namespace BathBombs.Components;

public record BathBombSpec : ComponentSpec
{

    [Serialize]
    public float Amount { get; init; }

    [Serialize]
    public bool Contaminated { get; init; }

    [Serialize]
    public int DetonationTicks { get; init; }

    [Serialize]
    public string ExplosionPrefabPath { get; init; } = null!;
}
