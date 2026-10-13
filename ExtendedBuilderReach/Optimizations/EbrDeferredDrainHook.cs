using HarmonyLib;

namespace ExtendedBuilderReach.Optimizations;

/// <summary>
/// Flushes deferred EBR-only notifications from the normal main-thread update.
/// No change to the game's tick buckets, path validation, or save pipeline.
/// </summary>
static class EbrDeferredDrainHook
{
    internal static bool Install(Harmony harmony)
    {
        var lifecycle = AccessTools.TypeByName("Timberborn.SingletonSystem.SingletonLifecycleService");
        var update = lifecycle is null ? null : AccessTools.Method(lifecycle, "UpdateSingletons");
        var callback = AccessTools.Method(typeof(EbrDeferredDrainHook), nameof(AfterUpdateSingletons));
        if (update is null || callback is null)
        {
            return false;
        }

        try
        {
            harmony.Patch(update, postfix: new HarmonyMethod(callback));
            return true;
        }
        catch
        {
            return false;
        }
    }

    static void AfterUpdateSingletons() =>
        ExtendedBuilderReachNavOptimizer.DrainDeferred();
}
