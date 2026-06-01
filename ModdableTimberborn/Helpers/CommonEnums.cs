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

[Flags]
public enum CharacterType
{
    Unknown = 0,

    Bot = 1 << 0,
    AdultBeaver = 1 << 1,
    ChildBeaver = 1 << 2,

    HighestBit = ChildBeaver,
    ArrayLength = HighestBit + 1,
    Beavers = AdultBeaver | ChildBeaver,
    Workers = AdultBeaver | Bot,
    All = Beavers | Bot
}
