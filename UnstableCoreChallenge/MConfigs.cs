
namespace UnstableCoreChallenge;

public class MConfigs : BaseModdableTimberbornConfiguration
{
    public override ConfigurationContext AvailableContexts { get; } = ConfigurationContext.MainMenu | ConfigurationContext.Game;

    public override void StartMod(IModEnvironment modEnvironment)
    {
        base.StartMod(modEnvironment);

        ModdableTimberbornRegistry.Instance
            .UseEntityTracker()
            .TryTrack<UnstableCoreStabilizer>()
            .TryTrack<Building>();
    }

    public override void Configure(Configurator configurator, ConfigurationContext context)
    {
        configurator.BindSingleton<MSettings>();

        if (!context.IsGameContext()) { return; }

        configurator
            .BindSingleton<CoreDisarmService>()
            .BindSingleton<UnstableCoreSpawner>()
            .BindSingleton<UnstableCoreService>()
            
            .BindFragment<StablizerFragment>()

            .MultiBindSingleton<IDevModule, UnstableCoreChallengeDevModule>()

            .BindTemplateModule(h => h
                .AddDecorator<UnstableCoreSpec, UnstableCoreStabilizer>()
            )
        ;
    }
}
