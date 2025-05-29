global using TailsManager.Models;
global using TailsManager.Services;

namespace TailsManager;

public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        this.BindSingleton<TailsManagerService>();
    }
}