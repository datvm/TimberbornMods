namespace MapResizer.Services;
public class SoilMapResizeService(
    SoilContaminationSimulator soilContaminationSimulator,
    SoilMoistureSimulator soilMoistureSimulator,
    IThreadSafeColumnTerrainMap threadSafeColumnTerrainMap,
    MapIndexService mapIndexService
)
{

    public void Resize()
    {
        var maxColumnCount = threadSafeColumnTerrainMap.MaxColumnCount;
        var verticalStride = mapIndexService.VerticalStride;
        var size = maxColumnCount * verticalStride;

        soilContaminationSimulator._contaminationLevels = new float[size];
        soilContaminationSimulator._contaminationCandidates = new float[size];

        soilMoistureSimulator._moistureLevels = new float[size];
    }

}
