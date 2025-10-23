namespace WeatherScientificProjects;

public class MConfig : BaseModdableTimberbornConfiguration
{

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .TryTrack<WeatherSPWaterStrengthModifier>();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.IsGameContext()) { return; }

        configurator
            .BindScientificProjectListener<EmergencyDrillService>()
            .BindScientificProjectListener<WeatherForecastService>()
            .BindScientificProjectListener<WeatherSPWarningExtender>()
            .BindScientificProjectListener<WeatherSPWaterStrengthService>(true)

            .MultiBindSingleton<IProjectCostProvider, WeatherSPCostProvider>()
            .MultiBindSingleton<IProjectUnlockConditionProvider, WeatherSPUnlockConditionProvider>()

            .BindSingleton<WeatherForecastPanel>()

            .BindTemplateModule(h => h
                .AddDecorator<WaterSourceContamination, WeatherSPWaterStrengthModifier>()
            );
        ;
    }
}

