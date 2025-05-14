namespace ScenarioEditor.Services.ScenarioTriggers;

public class TimeTrigger : IScenarioTrigger
{

#nullable disable
    GameCycleService gameCycleService;
#nullable enable

    public string NameKey { get; } = "LV.ScE.TimeTrigger";

    [Inject]
    public void Inject(GameCycleService gameCycleService)
    {
        this.gameCycleService = gameCycleService;
    }

    public int Cycle { get; set; }
    public int Day { get; set; }
    public float Hour { get; set; }
    
    public bool Check(ScenarioEvent scenarioEvent)
    {
        if (gameCycleService.Cycle < Cycle) { return false; }
        if (gameCycleService.Cycle > Cycle) { return true; }

        if (gameCycleService.CycleDay < Day) { return false; }
        if (gameCycleService.CycleDay > Day) { return true; }

        var hour = gameCycleService._dayNightCycle.HoursPassedToday;
        return hour >= Hour;
    }

}
