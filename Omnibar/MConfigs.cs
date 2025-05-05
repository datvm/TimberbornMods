global using Omnibar.Models;
global using Omnibar.Services;
global using Omnibar.Services.Providers;

namespace Omnibar;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this.MultiBindAndBindSingleton<IOmnibarProvider, OmnibarToolProvider>();

        Bind<ToDoListManager>().AsSingleton();
    }
}
