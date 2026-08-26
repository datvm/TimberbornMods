namespace CraneHeads.Components;

public enum TrebuchetShotStatus
{
    Valid,
    SameTile,
    OutOfRange,
    TooHigh,
    InvalidLanding,
    Blocked,
}

public readonly record struct TrebuchetShotCheck(TrebuchetShotStatus Status, int Distance, int MaxRange)
{
    public bool IsValid => Status == TrebuchetShotStatus.Valid;
}
