global using Ziporter.Components;
global using Ziporter.Services;
global using Ziporter.UI;

namespace Ziporter;

[Context("MainMenu")]
public class ModMenuConfig : Configurator
{
    public override void Configure()
    {
        this.TryAddingCameraShake(true);
    }

}

[Context("Game")]
public class GameConfig : Configurator
{
    public override void Configure()
    {
        this.TryAddingCameraShake(false);

        Bind<SunLightOverrider>().AsSingleton();

        Bind<ZiporterConnectionService>().AsSingleton();
        Bind<ZiporterNavGroupService>().AsSingleton();
        Bind<ZiporterConnectionTool>().AsSingleton();

        this.BindFragment<ZiporterFragment>();
        Bind<ZiporterConnectionButtonFactory>().AsSingleton();

        MultiBind<TemplateModule>().ToProvider(() =>
        {
            TemplateModule.Builder b = new();
            b.AddDecorator<ZiporterSpec, ZiporterController>();
            b.AddDecorator<ZiporterSpec, ZiporterLighting>();
            b.AddDecorator<ZiporterSpec, ZiporterBattery>();
            b.AddDecorator<ZiporterSpec, ZiporterStabilizer>();
            b.AddDecorator<ZiporterSpec, ZiporterConnection>();
            b.AddDecorator<ZiporterSpec, BatteryController>();

            return b.Build();
        }).AsSingleton();
    }
}

public class ModStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        new Harmony(nameof(Ziporter)).PatchAll();
    }

}
