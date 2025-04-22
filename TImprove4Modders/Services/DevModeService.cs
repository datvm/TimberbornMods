namespace TImprove4Modders.Services;

public class DevModeService(DevModeManager dev, EventBus eb) : ILoadableSingleton, IUnloadableSingleton
{
    public static bool IsDevModeOn = false;
    public static bool HasDevModeArgument = CommandLineArguments.CreateWithCommandLineArgs().Has("devMode");

    public void Load()
    {
        eb.Register(this);

        if (MSettings.DevModeOnDefault || HasDevModeArgument)
        {
            dev.Enable();
        }

        IsDevModeOn = dev.Enabled;
    }

    public void Unload()
    {
        eb.Unregister(this);
    }

    [OnEvent]
    public void OnDevModeChanged(DevModeToggledEvent e)
    {
        IsDevModeOn = e.Enabled;
    }
    
}
