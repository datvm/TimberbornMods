namespace TimberPipes.Models;

public static class DischargePipeIo
{
    public const float EjectSafety = 0.1f;

    public static bool IsWorldFluid(string? goodId)
        => goodId is PipeFluids.WaterGoodId or PipeFluids.BadwaterGoodId;

    public static bool IsContaminatedWater(string? goodId)
        => goodId == PipeFluids.BadwaterGoodId;

    public static bool ShouldContaminate(string? goodId)
        => goodId is { Length: > 0 }
            && goodId != PipeFluids.ContaminatedId
            && !IsWorldFluid(goodId);

    public static float AvailableSpace(int ejectZ, float waterHeightOrFloor, float distanceToGroundOffset)
        => ejectZ + distanceToGroundOffset - EjectSafety - waterHeightOrFloor;

    public static float EjectAmount(
        bool currentlyEjecting,
        float volume,
        string? goodId,
        float availableSpace,
        float ejectBuffer)
    {
        if (!IsWorldFluid(goodId) || volume <= PipeFluids.MoveEpsilon)
        {
            return 0f;
        }

        // Stop at the limit; only start again after EjectBuffer headroom (hysteresis).
        if (availableSpace <= 0f)
        {
            return 0f;
        }

        if (!currentlyEjecting && availableSpace < ejectBuffer)
        {
            return 0f;
        }

        var want = Math.Min(PipeFluids.PacketVolume, volume);
        var amount = Math.Min(want, availableSpace);
        return amount > PipeFluids.MoveEpsilon ? amount : 0f;
    }

    public static PipeContaminationCause WrongFluidCause(string goodId)
        => new(goodId, PipeFluids.WaterGoodId);
}
