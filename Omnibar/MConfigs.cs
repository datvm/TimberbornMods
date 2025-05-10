global using Omnibar.Models;
global using Omnibar.Services;
global using Omnibar.Services.Providers;
global using Omnibar.UI;
global using Omnibar.UI.TodoList;
global using Omnibar.Helpers;

namespace Omnibar;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this.MultiBindAndBindSingleton<IOmnibarProvider, OmnibarToolProvider>();
        MultiBind<IOmnibarProvider>().To<OmnibarMathProvider>().AsSingleton();

        Bind<OmnibarService>().AsSingleton();
        Bind<ToDoListManager>().AsSingleton();

        Bind<OmnibarController>().AsSingleton();
        Bind<OmnibarBox>().AsSingleton();

        Bind<TodoListController>().AsSingleton();
        Bind<TodoListPanel>().AsSingleton();
        Bind<TodoListDialog>().AsTransient();
        Bind<TodoListUpdater>().AsSingleton();
    }
}
