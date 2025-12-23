namespace ModdableWeathers.Services;

public class LandMoistureService : IUnloadableSingleton
{

    public static bool ShouldMoisturize { get; set; }

    public void Unload()
    {
        ShouldMoisturize = false;
    }
}
