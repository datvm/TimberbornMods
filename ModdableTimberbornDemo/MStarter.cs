using ModdableTimberbornDemo.Features.DI;
using ModdableTimberbornDemo.Features.EnterableBuff;
using ModdableTimberbornDemo.Features.MechanicalSystem;
using ModdableTimberbornDemo.Features.WorkplaceBuff;

namespace ModdableTimberbornDemo;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance
            // Buff configs
            .AddConfigurator<WorkplaceBuffConfig>()
            .AddConfigurator<EnterableBuffConfig>()

            // Mech System
            .UseMechanicalSystem()
            .AddConfigurator<MechanicalSystemConfig>()

            // DI: FactionServiceRunner should run after FactionSpecService is loaded but before FactionService is loaded
            .AddConfigurator<DIConfig>()
            .UseDependencyInjection(di => di
                .RegisterLoadTailRunner<FactionSpecService, DemoFactionServiceRunner>()

                .RegisterLoadFrontRunner<FactionService, DemoFactionServiceRunner>()                
                .RegisterLoadTailRunner<FactionService, DemoFactionServiceRunner>()
            )
        ;

    }

}
