namespace BuildingHP.Components;

public interface IBuildingDurabilityModifier
{

    event Action<IBuildingDurabilityModifier>? OnChanged;

    float? Multiplier { get; }
    int? Delta { get; }
    bool Invulnerable { get; }

}
