namespace PowerLines.Services;

public readonly record struct PowerLineConnectionCheck(
    bool CanConnect,
    float Distance,
    float MaxDistance,
    bool DistanceOk,
    bool HasSlot
);
