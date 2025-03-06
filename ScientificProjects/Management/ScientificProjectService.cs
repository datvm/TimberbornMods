namespace ScientificProjects.Management;

public partial class ScientificProjectService(
    ISingletonLoader loader,
    ScientificProjectRegistry registry,
    EventBus eb,
    ScienceService sciences
) : ILoadableSingleton, ISaveableSingleton, IUnloadableSingleton
{
    
}

