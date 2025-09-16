namespace ModdableTimberborn.DependencyInjection;

public sealed class DependencyInjectionRegistry
{
    record RunnerRegistryItem(Type TargetType, MethodInfo TargetMethod);
    class RegisterItemDict : Dictionary<Type, SortedSet<PriorityItem<RunnerRegistryItem>>> { }

    public const string PatchCategoryName = $"{nameof(ModdableTimberborn)}.{nameof(DependencyInjection)}";


    public static readonly DependencyInjectionRegistry Instance = new();

    private DependencyInjectionRegistry() { }

    SingletonLifecycleService? singletonLifecycleService;

    readonly RegisterItemDict
        loadFrontRunners = [],
        loadTailRunners = [],
        postLoadFrontRunners = [],
        postLoadTailRunners = [];

    DependencyInjectionRegistry Register<T, TRunner, TRunnerInterface>(RegisterItemDict dict, string methodName, int priority)
    {
        var list = dict.GetOrAdd(typeof(T));
        var item = new RunnerRegistryItem(typeof(TRunner), typeof(TRunnerInterface).Method(methodName));
        list.Add(new(item, priority));
        return this;
    }

    public DependencyInjectionRegistry RegisterLoadFrontRunner<T, TRunner>(int priority = 500)
        where T : ILoadableSingleton
        where TRunner : ILoadableSingletonFrontRunner<T> 
        => Register<T, TRunner, ILoadableSingletonFrontRunner<T>>(
            loadFrontRunners, nameof(ILoadableSingletonFrontRunner<>.FrontRun), priority);

    public DependencyInjectionRegistry RegisterLoadTailRunner<T, TRunner>(int priority = 500)
        where T : ILoadableSingleton
        where TRunner : ILoadableSingletonTailRunner<T> 
        => Register<T, TRunner, ILoadableSingletonTailRunner<T>>(
            loadTailRunners, nameof(ILoadableSingletonTailRunner<>.TailRun), priority);

    public DependencyInjectionRegistry RegisterPostLoadFrontRunner<T, TRunner>(int priority = 500)
        where T : IPostLoadableSingleton
        where TRunner : IPostLoadableSingletonFrontRunner<T>
        => Register<T, TRunner, IPostLoadableSingletonFrontRunner<T>>(
            postLoadFrontRunners, nameof(IPostLoadableSingletonFrontRunner<>.FrontRun), priority);

    public DependencyInjectionRegistry RegisterPostLoadTailRunner<T, TRunner>(int priority = 500)
        where T : IPostLoadableSingleton
        where TRunner : IPostLoadableSingletonTailRunner<T>
        => Register<T, TRunner, IPostLoadableSingletonTailRunner<T>>(
            postLoadTailRunners, nameof(IPostLoadableSingletonTailRunner<>.TailRun), priority);

    internal void StartRun(SingletonLifecycleService service)
    {
        singletonLifecycleService = service;
    }

    internal void FinishRun()
    {
        singletonLifecycleService = null;
    }

    internal void FrontRunLoad(ILoadableSingleton s)
        => InterceptRun(s, loadFrontRunners);
    internal void TailRunLoad(ILoadableSingleton s)
        => InterceptRun(s, loadTailRunners);
    internal void FrontRunPostLoad(IPostLoadableSingleton s)
        => InterceptRun(s, postLoadFrontRunners);
    internal void TailRunPostLoad(IPostLoadableSingleton s)
        => InterceptRun(s, postLoadTailRunners);

    static readonly MethodInfo GetSingletonsGenericMethod = typeof(SingletonRepository)
        .Method(nameof(SingletonRepository.GetSingletons));
    void InterceptRun<T>(T singleton, RegisterItemDict dict)
        where T : notnull
    {
        var singletonType = singleton.GetType();

        foreach (var (t, list) in dict)
        {
            if (!t.IsAssignableFrom(singletonType)) { continue; }

            foreach (var (runnerType, runnerMethod) in list.Select(p => p.Item))
            {
                var runners = (IEnumerable) GetSingletonsGenericMethod
                    .MakeGenericMethod([runnerType])
                    .Invoke(singletonLifecycleService!._singletonRepository, []);

                foreach (var runner in runners)
                {
                    runnerMethod.Invoke(runner, [singleton]);
                }
            }
        }
    }

}
