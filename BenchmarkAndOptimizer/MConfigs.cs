namespace BenchmarkAndOptimizer;

public class CommonConfig : Configurator
{
    public override void Configure()
    {
        this
            .BindSingleton<MSettings>()

            .BindSingleton<OptimizerSettings>()

            .MultiBindSingleton<IModSettingElementFactory, OptimizerModSettingFac>()
            .BindTransient<OptimizerPanel>()
        ;
    }
}

[Context("MainMenu")]
public class ModMenuConfig : CommonConfig
{
    public override void Configure()
    {
        base.Configure();
    }
}

[Context("Game")]
public class ModGameConfig : CommonConfig
{
    public override void Configure()
    {
        base.Configure();

        this
            .BindSingleton<GameOptimizerService>()
            .BindSingleton<GameBenchmarkService>()

            .BindSingleton<BenchmarkMenu>()
            .BindSingleton<BenchmarkDialog>()
        ;
    }
}
