
namespace Ziporter;

public class MConfigs : BaseModdableTimberbornConfigurationWithHarmony
{

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {        
        if (context.IsMenuContext())
        {
            configurator.TryBindingCameraShake(true);
        }

        if (!context.IsGameContext()) { return; }
        configurator.TryBindingCameraShake(false);

        configurator
            .BindSingleton<SunLightOverrider>()

            .BindSingleton<ZiporterConnectionService>()
            .BindSingleton<ZiporterNavGroupService>()
            .BindSingleton<ZiporterConnectionTool>()

            .BindFragment<ZiporterFragment>()
            .BindSingleton<ZiporterConnectionButtonFactory>()

            .BindTemplateModule(h => h
                .AddDecorator<ZiporterSpec, ZiporterController>()
                .AddDecorator<ZiporterSpec, ZiporterLighting>()
                .AddDecorator<ZiporterSpec, ZiporterBattery>()
                .AddDecorator<ZiporterSpec, ZiporterStabilizer>()
                .AddDecorator<ZiporterSpec, ZiporterConnection>()
                .AddDecorator<ZiporterSpec, ZiporterWarning>()
                .AddDecorator<ZiporterSpec, BatteryController>()
            )
        ;
    }
}