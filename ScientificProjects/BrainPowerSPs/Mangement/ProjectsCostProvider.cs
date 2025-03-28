namespace BrainPowerSPs.Mangement;

public class ProjectsCostProvider(EntityManager entities) : IProjectCostProvider
{
    public IEnumerable<string> CanCalculateCostForIds { get; } = [ModUtils.WaterWheelFlowUp2Id];

    public int CalculateCost(ScientificProjectInfo project)
    {
        var spec = project.Spec;

        return spec.Id switch
        {
            ModUtils.WaterWheelFlowUp2Id => this.LevelOr0F(project, level => spec.ScienceCost * level * CountWaterWheels()),
            _ => throw spec.ThrowNotSupportedEx(),
        };
    }

    int CountWaterWheels() => entities.Get<WaterPoweredGenerator>().Count;

}
