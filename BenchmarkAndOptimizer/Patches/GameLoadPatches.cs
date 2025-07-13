namespace BenchmarkAndOptimizer.Patches;

[HarmonyPatchCategory(MStarter.BenchmarkCategory), HarmonyPatch]
public static class GameLoadPatches
{
    readonly static LoadingBenchmarkService benchmark = LoadingBenchmarkService.Instance;

    [HarmonyPrefix, HarmonyPatch(typeof(SingletonLifecycleService), nameof(SingletonLifecycleService.LoadAll))]
    public static void LoadAllPrefix(SingletonLifecycleService __instance)
    {
        benchmark.Start(__instance);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(SingletonLifecycleService), nameof(SingletonLifecycleService.LoadAll))]
    public static void LoadAllPostfix()
    {
        benchmark.OnLoadAllPostfix();
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SingletonLifecycleService), nameof(SingletonLifecycleService.LoadSingletons))]
    public static bool LoadSingletonsPrefix()
    {
        benchmark.OnLoadSingletonsPrefix();
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SingletonLifecycleService), nameof(SingletonLifecycleService.LoadNonSingletons))]
    public static bool LoadNonSingletonsPrefix()
    {
        benchmark.OnLoadNonSingletonsPrefix();
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SingletonLifecycleService), nameof(SingletonLifecycleService.PostLoadSingletons))]
    public static bool PostLoadSingletonsPrefix()
    {
        benchmark.OnPostLoadSingletonsPrefix();
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(SingletonLifecycleService), nameof(SingletonLifecycleService.PostLoadNonSingletons))]
    public static bool PostLoadNonSingletonsPrefix()
    {
        benchmark.OnPostLoadNonSingletonsPrefix();
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(WorldEntitiesLoader), nameof(WorldEntitiesLoader.InstantiateEntities))]
    public static void InstantiateEntitiesPrefix(WorldEntitiesLoader __instance)
    {
        benchmark.OnInstantiateEntitiesPrefix(__instance);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(WorldEntitiesLoader), nameof(WorldEntitiesLoader.InstantiateEntities))]
    public static void InstantiateEntitiesPostfix(List<InstantiatedSerializedEntity> __result)
    {
        benchmark.OnInstantiateEntitiesPostfix(__result);
    }


}
