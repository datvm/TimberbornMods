namespace MapResizer.Services;

public class WaterMapResizeService(
    IThreadSafeWaterMap iThreadSafeWaterMap,
    WaterSimulator waterSimulator
)
{

    readonly ThreadSafeWaterMap threadSafeWaterMap = (ThreadSafeWaterMap)iThreadSafeWaterMap;

    public void Resize()
    {
        ResizeWaterMap();
        ResizeThreadSafeWaterMap();
        ResizeWaterEvaporationMap();
    }

    void ResizeWaterMap()
    {
        waterSimulator.MaxColumnCount = 1;
        waterSimulator.Load();
        waterSimulator.Update();
        waterSimulator.Reset();
    }

    void ResizeWaterEvaporationMap()
    {
        // Do nothing
    }

    void ResizeThreadSafeWaterMap()
    {
        threadSafeWaterMap.MaxColumnCount = 0;
        threadSafeWaterMap.Load();
        threadSafeWaterMap.PostLoad();
    }

}
