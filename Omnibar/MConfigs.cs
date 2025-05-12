namespace Omnibar;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this.MultiBindAndBindSingleton<IOmnibarProvider, OmnibarToolProvider>();
        MultiBind<IOmnibarProvider>().To<OmnibarMathProvider>().AsSingleton();
        MultiBind<IOmnibarProvider>().To<OmnibarBatchControlProvider>().AsSingleton();
        MultiBind<IOmnibarProvider>().To<OmnibarRecipeProvider>().AsSingleton();
        MultiBind<IOmnibarProvider>().To<OmnibarBeaverProvider>().AsSingleton();

        // Special provider, do not multibind to the interface
        Bind<OmnibarCommandProvider>().AsSingleton();

        Bind<OmnibarService>().AsSingleton();
        Bind<ToDoListManager>().AsSingleton();

        Bind<OmnibarController>().AsSingleton();
        Bind<OmnibarBox>().AsSingleton();

        Bind<TodoListController>().AsSingleton();
        Bind<TodoListPanel>().AsSingleton();        
        Bind<TodoListUpdater>().AsSingleton();

        Bind<TodoListDialog>().AsTransient();
        Bind<ToDoListEntryEditor>().AsTransient();
    }
}
