namespace PowerOverdrive.Services;

public readonly record struct PowerOverdriveLevel(
    int Level,
    string ItemId,
    float ProductionBonus,
    float PowerUsageBonus,
    GlobalGoodHandle Good
);
