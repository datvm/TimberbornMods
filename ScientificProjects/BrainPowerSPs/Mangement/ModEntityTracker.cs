global using Timberborn.PowerGenerating;

namespace BrainPowerSPs.Mangement;

public class ModEntityTracker : ITrackingEntities
{
    public IEnumerable<Type> TrackingTypes { get; } = [typeof(WaterPoweredGenerator)];
}
