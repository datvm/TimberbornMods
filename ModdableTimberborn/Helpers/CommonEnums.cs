namespace ModdableTimberborn.Helpers;

public enum BonusType
{
    CarryingCapacity,
    CuttingSuccessChance,
    GrowthSpeed,
    LifeExpectancy,
    MovementSpeed,
    WorkingSpeed
}

public enum ModifierPriority : int
{
    Force = -1000,
    Additive = 0,
    Multiplicative = 1000,
}