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

        soilContaminationSimulator._contaminationLevels = soilContaminationSimulator._tickOnlyArrayService.Create<float>(size);
        soilContaminationSimulator._contaminationCandidates = soilContaminationSimulator._tickOnlyArrayService.Create<float>(size);

        soilMoistureSimulator._moistureLevels = soilMoistureSimulator._tickOnlyArrayService.Create<float>(size);
    }

}
