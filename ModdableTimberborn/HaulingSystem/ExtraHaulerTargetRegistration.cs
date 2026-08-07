namespace ModdableTimberborn.HaulingSystem;

/// <summary>
/// Registration for an inventory that haulers may fill in the listed districts.
/// </summary>
public readonly record struct ExtraHaulerTargetRegistration(
    Inventory Inventory,
    IReadOnlyList<DistrictCenter> Districts,
    Accessible? Accessible = null,
    float Weight = 1f,
    bool OnlyInputGoods = true
);
