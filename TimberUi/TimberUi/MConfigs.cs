namespace TimberUi;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        this.BindSingleton<ModUpdateService>();
    }
}

[Context("MainMenu")]
[Context("Game")]
[Context("MapEditor")]
public class ModAllConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<GameSliderAlternativeManualValueDI>()
            .BindSingleton<DialogService>()
        ;
    }
}