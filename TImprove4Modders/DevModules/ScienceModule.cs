global using Timberborn.ScienceSystem;

namespace TImprove4Modders.DevModules;

public class ScienceModule(ScienceService sciences) : IDevModule
{

    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.Create("Science: Set to 0", SetTo0))
            .AddMethod(DevMethod.Create("Science: Set to 100", SetTo100))
            .AddMethod(DevMethod.Create("Science: Add 100", Add100))
            .Build();
    }

    public void SetTo0()
    {
        sciences.SubtractPoints(sciences.SciencePoints);
    }
    
    public void SetTo100()
    {
        SetTo0();
        Add100();
    }

    public void Add100()
    {
        sciences.AddPoints(100);
    }

}
