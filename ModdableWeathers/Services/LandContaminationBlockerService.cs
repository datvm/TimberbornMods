namespace ModdableWeathers.Services;

public class LandContaminationBlockerService : IUnloadableSingleton
{

    public static bool ShouldBlock { get; set; }

    public void Unload()
    {
        ShouldBlock = false;
    }
}
