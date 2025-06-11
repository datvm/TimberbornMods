namespace TimberUi;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        this.BindSingleton<ModUpdateService>();
    }
}
