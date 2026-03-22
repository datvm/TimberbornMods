namespace RamPump.Services;

public class RamPumpService(
    IWaterService waterService,
    IThreadSafeWaterMap waterMap,
    ITickService tickService,
    ILoc t,
    IDayNightCycle dayNightCycle
) : ILoadableSingleton
{

    float secondsPerTick; // generally 0.6s per tick
    float gameHoursPerSecond;

    public void Load()
    {
        secondsPerTick = tickService.TickIntervalInSeconds;

        var gameHoursPerTick = dayNightCycle.HoursToTicks(1);
        gameHoursPerSecond = 1 / (secondsPerTick * gameHoursPerTick);
    }

    public string GetStrengthDescription(float strength)
        => t.T("LV.RHP.RamPump.StrengthDesc", strength, gameHoursPerSecond / strength);

    public (float Total, float Contamination, float CurrentPerTick) GetWaterAt(Vector3Int coord)
    {
        var available = waterMap.WaterHeightOrFloor(coord) - coord.z;
        if (available <= 0f) { return default; }

        var current = waterMap.WaterFlowDirection(coord).magnitude; // m3/s
        if (current <= 0f) { return default; }

        current *= secondsPerTick; // now m3/tick
        var contamination = waterMap.ColumnContamination(coord);
        return (available, contamination, current);
    }

    public void RemoveWater(Vector3Int coord, float clean, float contaminated)
    {
        if (clean > 0)
        {
            waterService.RemoveCleanWater(coord, clean);
        }
            
        if (contaminated > 0)
        {
            waterService.RemoveContaminatedWater(coord, contaminated);
        }   
    }

    public void AddWater(Vector3Int coord, float clean, float contaminated)
    {
        if (clean > 0)
        {
            waterService.AddCleanWater(coord, clean);
        }
        
        if (contaminated > 0)
        {
            waterService.AddContaminatedWater(coord, contaminated);
        }
    }

    public void AddWaterLimit(RamPumpComponent comp, BlockObject bo)
    {
        waterService.AddDirectionLimiter(comp.InputCoord, bo.Orientation.ToFlowDirection());
        foreach (var c in GetBlockingCoordinates(comp, bo))
        {
            waterService.AddFullObstacle(c);
        }
    }

    public void RemoveWaterLimit(RamPumpComponent comp)
    {
        var bo = comp.GetComponent<BlockObject>();

        waterService.RemoveDirectionLimiter(comp.InputCoord);
        foreach (var c in GetBlockingCoordinates(comp, bo))
        {
            waterService.RemoveFullObstacle(c);
        }
    }

    IEnumerable<Vector3Int> GetBlockingCoordinates(RamPumpComponent comp, BlockObject bo)
    {
        foreach (var c in comp.Spec.WaterBlockingCoordinates)
        {
            yield return bo.TransformCoordinates(c);
        }
    }


}
