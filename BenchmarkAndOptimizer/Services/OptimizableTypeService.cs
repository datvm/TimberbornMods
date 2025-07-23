namespace BenchmarkAndOptimizer.Services;

public static class OptimizableTypeService
{
    public const string UpdateMethod = nameof(StreamGauge.Update);

    static readonly ImmutableArray<Type> OptimizableSingletons = [
        typeof(IUpdatableSingleton), typeof(ILateUpdatableSingleton),
            typeof(ITickableSingleton),
    ];

    public static ImmutableHashSet<Type> OptimizableTypesLookup { get; private set; } = [];
    public static ImmutableArray<Type> OptimizableTypes { get; private set; } = [];
    public static readonly ImmutableHashSet<Type> WellKnownTypes = [
        typeof(BehaviorManager), typeof(NavMeshObserver), typeof(ConstructionSite), 
        typeof(BuilderJobReachabilityStatus), typeof(ResourceCountingService),
    ];

    public static IEnumerable<Type> UpdatableComponents =>
        OptimizableTypes.Where(t => typeof(BaseComponent).IsAssignableFrom(t) && t.Method(UpdateMethod) is not null);

    static OptimizableTypeService()
    {
        Scan();
    }

    static void Scan()
    {
        if (OptimizableTypesLookup.Count > 0) { return; }

        OptimizableTypesLookup = [.. ScanForTypes()];
        OptimizableTypes = [.. OptimizableTypesLookup.OrderBy(q => q.Name)];
    }

    static IEnumerable<Type> ScanForTypes()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            foreach (var t in asm.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface) { continue; }

                if (IsOptimizableComponent(t) || IsOptimizableSingleton(t))
                {
                    yield return t;
                }
            }
        }
    }

    static bool IsOptimizableComponent(Type t) =>
        typeof(BaseComponent).IsAssignableFrom(t)
        && (
            typeof(TickableComponent).IsAssignableFrom(t)
            || t.GetMethod("Update") != null
            || t.GetMethod("LateUpdate") != null
        );

    static bool IsOptimizableSingleton(Type t)
    {
        foreach (var i in OptimizableSingletons)
        {
            if (i.IsAssignableFrom(t))
            {
                return true;
            }
        }

        return false;
    }


}
