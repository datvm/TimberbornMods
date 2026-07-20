namespace BuildingHP.Models;

public readonly record struct BuildingHPModels(int HP, int Durability)
{

    public BuildingHPModels(BuildingHPComponent comp) : this(comp.HP, comp.Durability) { }

}

public readonly record struct BuildingDurabilityDescription(
    string DescriptionKey,
    float Value,
    BuildingDurabilityModifierType Type,
    float? EndTime = null
);

public enum BuildingDurabilityModifierType
{
    Addition,
    Multiplier,
    Invulnerability
}

public readonly record struct BuildingRepairInfo(string Good, int Amount, float HPPercent)
{

    public static readonly BuildingRepairInfo OneLog = new("Log", 1, BuildingRepairService.RepairMaxPortion);

}
