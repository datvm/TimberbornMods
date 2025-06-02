namespace TimberUi.Services;

public interface ISystemFileDialogService
{

    string? ShowOpenFileDialog(string? filter = default);
    string? ShowSaveFileDialog(string? filter = default);

    public static Configurator TryBinding(Configurator configurator)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            configurator.TryBind<ISystemFileDialogService>()?.To<WindowsSystemFileDialogService>().AsSingleton();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            configurator.TryBind<ISystemFileDialogService>()?.To<LinuxSystemFileDialogService>().AsSingleton();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            configurator.TryBind<ISystemFileDialogService>()?.To<MacOSSystemFileDialogService>().AsSingleton();
        }
        else
        {
            Debug.LogError($"Unsupported platform for {nameof(ISystemFileDialogService)}.");
        }

        return configurator;
    }

}
