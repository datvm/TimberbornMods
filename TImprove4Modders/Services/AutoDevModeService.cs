namespace TImprove4Modders.Services;

public class AutoDevModeService(DevModeManager dev) : ILoadableSingleton
{

    public void Load()
    {
        if (MSettings.DevModeOnDefault)
        {
            dev.Enable();
        }
    }

}
