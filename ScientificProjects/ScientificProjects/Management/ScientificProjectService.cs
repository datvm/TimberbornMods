
namespace ScientificProjects.Management;

public partial class ScientificProjectService(
    ISingletonLoader loader,
    ScientificProjectRegistry registry,
    EventBus eb,
    ScienceService sciences,
    ILoc t,
    FactionService factions
) : ILoadableSingleton, ISaveableSingleton, IUnloadableSingleton
{
    
}

