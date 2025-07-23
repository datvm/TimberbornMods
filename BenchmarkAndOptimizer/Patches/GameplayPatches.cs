namespace BenchmarkAndOptimizer.Patches;

[HarmonyPatchCategory(MStarter.OptimizeCategory), HarmonyPatch, HarmonyPriority(300)]
public static class GameplayPatches
{

    [HarmonyPrefix, HarmonyPatch(typeof(TickableSingletonService), nameof(TickableSingletonService.TickSingletons))]
    public static bool TickSingletons(TickableSingletonService __instance)
        => Delegate(__instance._tickableSingletons, s => s.Tick(), s => s._tickableSingleton);

    [HarmonyPrefix, HarmonyPatch(typeof(TickableEntity), nameof(TickableEntity.TickTickableComponents))]
    public static bool TickTickableComponents(TickableEntity __instance) =>
        Delegate(__instance._tickableComponents, static c =>
        {
            if (c.Enabled)
            {
                c.StartAndTick();
            }
        }, c => c._tickableComponent);

    [HarmonyPrefix, HarmonyPatch(typeof(SingletonLifecycleService), nameof(SingletonLifecycleService.UpdateSingletons))]
    public static bool UpdateSingletons(SingletonLifecycleService __instance)
        => Delegate(__instance._updatableSingletons, s => s.UpdateSingleton());

    [HarmonyPrefix, HarmonyPatch(typeof(SingletonLifecycleService), nameof(SingletonLifecycleService.LateUpdateSingletons))]
    public static bool LateUpdateSingletons(SingletonLifecycleService __instance)
        => Delegate(__instance._lateUpdatableSingletons, s => s.LateUpdateSingleton());

    public static bool UpdateComponentPrefix(BaseComponent __instance, ref BenchmarkTracker? __state)
    {
        if (GameOptimizerService.Instance is null) { return true; }

        var (shouldRun, tracker) = GameOptimizerService.Instance.RunComponentUpdate(__instance);
        __state = tracker;

        return shouldRun;
    }

    public static void UpdateComponentPostfix(BenchmarkTracker? __state) => __state?.End();

    static bool Delegate<T>(IList<T> list, Action<T> action) where T : notnull =>
        GameOptimizerService.Instance?
            .RunList(list, action)
            ?? true;

    static bool Delegate<T, TActual>(IList<T> list, Action<T> action, Func<T, TActual> trackingType) where TActual : notnull =>
        GameOptimizerService.Instance?
            .RunList(list, action, trackingType)
            ?? true;

}
