using ExtendedBuilderReach.Optimizations;

namespace ExtendedBuilderReach;

public class MStarter : IModStarter
{
    public void StartMod(IModEnvironment modEnvironment)
    {
        var harmony = new Harmony(nameof(ExtendedBuilderReach));
        harmony.PatchAll();

        // A missing frame hook must never leave deferred navigation work stalled.
        // If this game version does not expose the update hook, retain vanilla EBR notifications.
        if (EbrDeferredDrainHook.Install(harmony))
        {
            ExtendedBuilderReachNavOptimizer.Patch(harmony);
        }
    }
}
