using Timberborn.GameDistricts;

namespace ColorfulBeavers.Graphical;

public class CitizenDistrictTintResettter : BaseComponent
{
    
    public void Start()
    {
        var district = GetComponentFast<DistrictPopulation>();

        RemoveDistrictEvent(nameof(DistrictPopulation.CitizenAssigned), district);
        RemoveDistrictEvent(nameof(DistrictPopulation.CitizenUnassigned), district);
    }

    void RemoveDistrictEvent(string evName, DistrictPopulation district)
    {
        var ev = typeof(DistrictPopulation).GetEvent(evName);
        var evField = typeof(DistrictPopulation).GetField(nameof(DistrictPopulation.CitizenAssigned), BindingFlags.NonPublic | BindingFlags.Instance);
        var evDelegate = (Delegate)evField.GetValue(district)!;

        foreach (var d in evDelegate.GetInvocationList().ToList())
        {
            if (d.Target.ToString().Contains(nameof(CitizenDistrictTintChanger)))
            {
                ev.RemoveEventHandler(district, d);
            }
        }
    }

}
