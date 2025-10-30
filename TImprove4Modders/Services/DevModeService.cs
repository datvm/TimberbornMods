namespace TImprove4Modders.Services;

public class DevModeService(
    DevModeManager dev,
    EventBus eb,
    InputService inputService
) : ILoadableSingleton
{
    public static bool IsDevModeOn = false;
    public static bool HasDevModeArgument = CommandLineArguments.CreateWithCommandLineArgs().Has("devMode");

    readonly InputService inputService = inputService;

    public void Load()
    {
        eb.Register(this);

        if (MSettings.DevModeOnDefault || HasDevModeArgument)
        {
            dev.Enable();
        }

        IsDevModeOn = dev.Enabled;
    }

    [OnEvent]
    public void OnDevModeChanged(DevModeToggledEvent e)
    {
        IsDevModeOn = e.Enabled;
    }



}
