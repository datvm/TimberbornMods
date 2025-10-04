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

            // DI
            .UseDependencyInjection()
            .AddConfigurator<DemoDIConfig>()
        ;

    }

}
