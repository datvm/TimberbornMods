namespace MoreBuildingRenovations.Services;

[BindSingleton]
public class NightlyScienceService(
    HourlyEventService hourlyEventService,
    ScienceService scienceService
) : ILoadableSingleton
{

    public int ExtraSciencePerBuilding { get; set; }
    readonly DeferredHashSet<BuildingRenovationComponent> nightlyScienceBuildings = [];

    public void Load()
    {
        hourlyEventService.OnNewHour += OnNewHour;
    }

    void OnNewHour(object sender, NewHourEventArgs e)
    {
        if (!e.IsNightTime || nightlyScienceBuildings.Count == 0) { return; }

        var extraSciencePer = ExtraSciencePerBuilding;
        var extraScience = 0;
        foreach (var b in nightlyScienceBuildings)
        {
            if (!b)
            {
                nightlyScienceBuildings.Remove(b);
                continue;
            }

            extraScience += extraSciencePer;
        }

        if (extraScience > 0)
        {
            scienceService.AddPoints(extraScience);
        }
    }

    public void Add(BuildingRenovationComponent building)
    {
        nightlyScienceBuildings.Add(building);
    }

}
