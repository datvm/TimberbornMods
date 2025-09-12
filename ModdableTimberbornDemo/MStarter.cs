using ModdableTimberbornDemo.Features.EnterableBuff;
using ModdableTimberbornDemo.Features.WorkplaceBuff;

namespace ModdableTimberbornDemo;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        ModdableTimberbornRegistry.Instance
            .AddConfigurator<WorkplaceBuffConfig>()
            .AddConfigurator<EnterableBuffConfig>()
        ;

    }

}
