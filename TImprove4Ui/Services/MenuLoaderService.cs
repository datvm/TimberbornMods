namespace TImprove4Ui.Services;

public class MenuLoaderService(ILoc t) : ILoadableSingleton
{

    public static string? PauseStatusDescription { get; private set; }

    public void Load()
    {
        PauseStatusDescription ??= t.T(PausableBuilding.PausedLocKey);
    }
}
