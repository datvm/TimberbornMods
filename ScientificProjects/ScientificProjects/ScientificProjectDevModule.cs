namespace ScientificProjects;

public class ScientificProjectDevModule(IDayNightCycle cycle) : IDevModule
{
    const string JumpToNextDayKey = "JumpToNextDay";

    public DevModuleDefinition GetDefinition()
    {
        return new DevModuleDefinition.Builder()
            .AddMethod(DevMethod.CreateBindable("Jump to next day", JumpToNextDayKey, JumpToNextDay))
            .Build();
    }

    public void JumpToNextDay()
    {
        cycle.SetTimeToNextDay();
    }

}
