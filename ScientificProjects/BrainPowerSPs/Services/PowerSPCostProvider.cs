namespace BrainPowerSPs.Services;

public class PowerSPCostProvider(DefaultEntityTracker<WaterWheelPowerSPComponent> tracker) : IProjectCostProvider
{
    public IEnumerable<string> CanCalculateCostForIds { get; } = [PowerProjectsUtils.WaterWheelFlowUp2Id];

    public int CalculateCost(ScientificProjectSpec project, int level) 
        => project.Id switch
        {
            PowerProjectsUtils.WaterWheelFlowUp2Id => this.LevelOr0F(level, l => project.ScienceCost * l * CountWaterWheels()),
            _ => throw project.ThrowNotSupportedEx(),
        };

    int CountWaterWheels() => tracker.Entities.Count;

}
