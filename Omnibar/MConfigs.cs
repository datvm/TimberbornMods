namespace Omnibar;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .MultiBindAndBindSingleton<IOmnibarProvider, OmnibarToolProvider>()

            .MultiBindSingleton<IOmnibarProvider, OmnibarMathProvider>()
            .MultiBindSingleton<IOmnibarProvider, OmnibarBatchControlProvider>()
            .MultiBindSingleton<IOmnibarProvider, OmnibarRecipeProvider>()
            .MultiBindSingleton<IOmnibarProvider, OmnibarFindEntityProvider>()
            .MultiBindSingleton<IOmnibarProvider, OmnibarTodoListProvider>()

            // Special provider, do not multibind to the interface
            .BindSingleton<OmnibarCommandProvider>()

            .BindSingleton<OmnibarService>()
            .BindSingleton<TodoListManager>()

            .BindSingleton<OmnibarController>()
            .BindSingleton<OmnibarBox>()

            .BindSingleton<TodoListController>()
            .BindSingleton<TodoListPanel>()
            .BindSingleton<TodoListUpdater>()

            .BindTransient<TodoListDialog>()
            .BindTransient<TodoListEntryEditor>()
            .BindTransient<TodoListEntryBuildingEditor>()
            .BindTransient<TodoListItemSelectorDialog>()

            // Hotkey provider
            .MultiBindSingleton<IOmnibarHotkeyProvider, TodoListHotkeyProvider>()
        ;
    }
}
