namespace QuickBar;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .MultiBindSingleton<IOmnibarHotkeyProvider, OmnibarQuickbarProvider>()

            .BindSingleton<QuickBarPersistentService>()
            .BindSingleton<QuickBarHotkeyService>()
            .BindSingleton<QuickBarService>()

            .BindSingleton<QuickBarController>()
            .BindSingleton<QuickBarElement>()

            .MultiBindSingleton<IQuickBarItemProvider, ToolQuickBarItemProvider>()
            .MultiBindAndBindSingleton<IQuickBarItemProvider, EntityQuickBarItemProvider>()
        ;
    }
}
