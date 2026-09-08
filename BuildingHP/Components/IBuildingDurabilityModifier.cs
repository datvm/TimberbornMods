namespace BuildingHP.Components;

public interface IBuildingDurabilityModifier
{

    void Initialize();

    event Action<IBuildingDurabilityModifier>? OnChanged;
    string DescriptionKey { get; }
    float? ModifierEndTime { get; }

}

public interface IBuildingDeltaDurabilityModifier : IBuildingDurabilityModifier
{
    int? Delta { get; }
}

public interface IBuildingMultiplierDurabilityModifier : IBuildingDurabilityModifier
{
    float? Multiplier { get; }
}

public interface IBuildingInvulnerabilityModifier : IBuildingDurabilityModifier
{
    bool Invulnerable { get; }
}
