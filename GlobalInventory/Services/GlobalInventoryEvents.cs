namespace GlobalInventory.Services;

public record GlobalGoodAmountChangedEvent(
    GlobalGoodHandle Handle, float Previous, float Current, float AppliedDelta);

public record GlobalGoodCapacityChangedEvent(
    GlobalGoodHandle Handle, float Min, float? Max);

public record GlobalGoodVisibilityChangedEvent(
    GlobalGoodHandle Handle, bool Visible);

public record GlobalGoodClickedEvent(GlobalGoodHandle Handle);
