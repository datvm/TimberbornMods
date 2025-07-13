namespace BenchmarkAndOptimizer.Services;

public class LoadingBenchmarkService : BaseBenchmarkService
{
    public static readonly LoadingBenchmarkService Instance = new();

    SingletonLifecycleService singletonLifecycle = null!;
    WorldEntitiesLoader worldEntitiesLoader = null!;

    public void Start(SingletonLifecycleService singletonLifecycle)
    {
        this.singletonLifecycle = singletonLifecycle;
        StartTime = LastTime = DateTime.Now;

        ModUtils.Log(() => $"Loading benchmark started!");
        MarkTime("LoadAllPrefix");
    }

    #region SingletonLifecycleService events

    public void OnLoadSingletonsPrefix()
    {
        if (singletonLifecycle is null) { return; }

        LogSingletonItems();

        BenchmarkList(singletonLifecycle._loadableSingletons, "LoadSingletons", "LoadSingleton", s => s.Load());
    }

    public void OnLoadNonSingletonsPrefix()
    {
        if (singletonLifecycle is null) { return; }

        BenchmarkList(singletonLifecycle._nonSingletonLoaders, "LoadNonSingletons", "LoadNonSingleton", s => s.LoadNonSingletons());
    }

    public void OnPostLoadSingletonsPrefix()
    {
        if (singletonLifecycle is null) { return; }

        BenchmarkList(singletonLifecycle._postLoadableSingletons, "PostLoadSingletons", "PostLoadSingleton", s => s.PostLoad());
    }

    public void OnPostLoadNonSingletonsPrefix()
    {
        if (singletonLifecycle is null) { return; }

        BenchmarkList(singletonLifecycle._nonSingletonPostLoaders, "PostLoadNonSingletons", "PostLoadNonSingleton", s => s.PostLoadNonSingletons());
    }

    void LogSingletonItems()
    {
        var time = MarkTime("LoadSingletonsPrefix");
        ModUtils.Log(() => $"Retrieving singletons lists: {time.ToLogString()}");

        StringBuilder list = new();
        list.AppendLine();

        list.AppendLine($"{nameof(singletonLifecycle._loadableSingletons)}: {singletonLifecycle._loadableSingletons.Length}");
        list.AppendLine($"{nameof(singletonLifecycle._nonSingletonLoaders)}: {singletonLifecycle._nonSingletonLoaders.Length}");
        list.AppendLine($"{nameof(singletonLifecycle._postLoadableSingletons)}: {singletonLifecycle._postLoadableSingletons.Length}");
        list.AppendLine($"{nameof(singletonLifecycle._nonSingletonPostLoaders)}: {singletonLifecycle._nonSingletonPostLoaders.Length}");
        list.AppendLine($"{nameof(singletonLifecycle._unloadableSingletons)}: {singletonLifecycle._unloadableSingletons.Length}");
        list.AppendLine($"{nameof(singletonLifecycle._updatableSingletons)}: {singletonLifecycle._updatableSingletons.Length}");
        list.AppendLine($"{nameof(singletonLifecycle._lateUpdatableSingletons)}: {singletonLifecycle._lateUpdatableSingletons.Length}");
        ModUtils.Log(list.ToString);
    }

    public void OnLoadAllPostfix()
    {
        ModUtils.Log(() => $"Loading benchmark ended. Total loading time: {(DateTime.Now - StartTime).ToLogString()}");
    }

    #endregion

    #region WorldEntitiesLoader events

    const string InstantiateEntitiesMark = $"{nameof(WorldEntitiesLoader)}.{nameof(WorldEntitiesLoader.InstantiateEntities)}";
    public void OnInstantiateEntitiesPrefix(WorldEntitiesLoader instance)
    {
        worldEntitiesLoader = instance;
        MarkTime(InstantiateEntitiesMark);
        ModUtils.Log(() => $"Instantiating entities...");
    }

    public void OnInstantiateEntitiesPostfix(List<InstantiatedSerializedEntity> list)
    {
        var duration = GetTotalFor(InstantiateEntitiesMark);
        ModUtils.Log(() => $"Instantiating {list.Count} entities took {duration.ToLogString()}");
        CountEntities(list, item => item.TemplateName);
    }

    #endregion

}
