namespace BeaverChronicles.Services;

[BindSingleton]
public class PopulationHelper(
    SamplingPopulationService samplingPopulationService
)
{

    public BedData GetBeaverBedData(DistrictCenter? dc = null)
        => GetPopulationData(dc).BedData;

    public float GetShelteredRatio(DistrictCenter? dc = null)
    {
        var data = GetBeaverBedData(dc);
        var totalBeavers = data.OccupiedBeds + data.Homeless;

        return totalBeavers == 0 ? 0 : Mathf.Clamp01((float)data.OccupiedBeds / totalBeavers);
    }

    public PopulationData GetPopulationData(DistrictCenter? dc = null)
        => dc ? samplingPopulationService.GetDistrictData(dc) : samplingPopulationService.GlobalPopulationData;

}
