namespace ScientificProjects.Services.Dev;

public class DefaultSpDevModule(
    ScientificProjectUnlockRegistry unlocks,
    DialogService diag,
    ScienceService sciences,
    ScientificProjectDailyService dailyService
) : ISPDevModule
{

    public IEnumerable<SPDevEntry> GetEntries()
    {
        return [
            new("Clear all unlocks", DevClearAllUpgrades),
            new("Set science to 0", () => SetScienceTo(0)),
            new("Set science to Daily Cost", () => SetScienceTo(dailyService.CalculateDayCost())),
            new("Set science to value", SetScienceTo)
        ];
    }

    void DevClearAllUpgrades()
    {
        unlocks.Clear();
        diag.Alert("Save and reload to be sure.");
    }

    void SetScienceTo(int science) => sciences.SciencePoints = science;

    async void SetScienceTo()
    {
        var value = await diag.PromptAsync("Enter value: ");
        if (value is not null && int.TryParse(value, out var v))
        {
            SetScienceTo(v);
        }
    }

}
