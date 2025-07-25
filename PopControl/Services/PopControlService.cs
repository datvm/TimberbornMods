namespace PopControl.Services;

public class PopControlService(
    PopControlRegistry registry,
    BeaverPopulation beaverPopulation,
    BotPopulation botPopulation
) : ILoadableSingleton, IUnloadableSingleton
{
    public static PopControlService? Instance { get; private set; }

    public bool ShouldPreventBreeding(ProcreationHouse procreationHouse)
        => ShouldPreventBreeding(procreationHouse.GetComponentFast<DistrictBuilding>, true);

    public bool ShouldPreventBreeding(DistrictBuilding? districtBuilding, bool isBeaver) 
        => ShouldPreventBreeding(() => districtBuilding, isBeaver);

    public bool ShouldPreventBreeding(Func<DistrictBuilding?> districtBuilding, bool isBeaver)
    {
        if (ShouldNotBreed(isBeaver, null)) { return true; }

        var building = districtBuilding();
        if (!building) { return false; }

        var districtCenter = building.District;
        if (!districtCenter) { return false; }

        return ShouldNotBreed(isBeaver, districtCenter);
    }

    bool ShouldNotBreed(bool isBeaver, DistrictCenter? districtCenter)
    {
        var data = districtCenter ? registry.GetControlFor(districtCenter) : registry.Global;

        if (isBeaver)
        {
            if (!data.LimitBeavers || data.Beavers == 0) { return false; }
        }
        else
        {
            if (!data.LimitBots || data.Beavers == 0) { return false; }
        }

        var pop = districtCenter
            ? GetPop(districtCenter, isBeaver)
            : GetGlobalPop(isBeaver);

        return isBeaver ? pop >= data.Beavers : pop >= data.Bots;
    }

    int GetGlobalPop(bool isBeaver) => isBeaver
            ? beaverPopulation.NumberOfBeavers
            : botPopulation.NumberOfBots;

    int GetPop(DistrictCenter districtCenter, bool isBeaver)
    {
        var pop = districtCenter.DistrictPopulation;

        return isBeaver
            ? pop._beaverCollection._beavers.Count
            : pop._bots.Count;
    }

    public void Load()
    {
        Instance = this;
    }

    public void Unload()
    {
        Instance = null;
    }
}
