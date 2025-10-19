namespace ColorfulZipline;

[Context("Game")]
public class ModGameConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<ZiplineColoringService>()

            .BindFragment<ColorfulZiplineFragment>()

            .BindTemplateModule(h => h
                .AddDecorator<ZiplineTower, ZiplineTowerColorComponent>()
            )
        ;
    }
}
