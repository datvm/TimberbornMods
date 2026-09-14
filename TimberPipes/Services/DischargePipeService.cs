namespace TimberPipes.Services;

[BindSingleton]
public class DischargePipeService(
    IWaterService waterService,
    IThreadSafeWaterMap waterMap,
    ILoc t)
{
    public readonly ILoc t = t;

    public float AvailableSpace(Vector3Int ejectCell, float distanceToGroundOffset)
        => DischargePipeIo.AvailableSpace(
            ejectCell.z,
            waterMap.WaterHeightOrFloor(ejectCell),
            distanceToGroundOffset);

    public float EjectAmount(
        bool currentlyEjecting,
        float volume,
        string? goodId,
        float availableSpace,
        float ejectBuffer)
        => DischargePipeIo.EjectAmount(currentlyEjecting, volume, goodId, availableSpace, ejectBuffer);

    public void AddWorldWater(
        WaterOutput? waterOutput,
        Vector3Int ejectCell,
        float clean,
        float contaminated)
    {
        if (waterOutput)
        {
            waterOutput!.AddWater(clean, contaminated);
            return;
        }

        if (clean > 0)
        {
            waterService.AddCleanWater(ejectCell, clean);
        }

        if (contaminated > 0)
        {
            waterService.AddContaminatedWater(ejectCell, contaminated);
        }
    }
}
