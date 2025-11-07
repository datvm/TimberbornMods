using System.Reflection.Emit;
using Timberborn.GoodStackSystem;

namespace ExtendedBuilderReach.Patches;

[HarmonyPatch]
public static class DemolishableAccessiblePatch
{

    [HarmonyPostfix, HarmonyPatch(typeof(ReachableDemolishable), nameof(ReachableDemolishable.Start))]
    public static void PatchReachableDemolishableAccessible(ReachableDemolishable __instance)
    {
        if (__instance._accessible
            || !MSettings.ExtendDemolishValue
            || !__instance.HasComponent<Demolishable>()
            || !__instance.HasComponent<GoodStackAccessible>()) { return; }

        __instance._accessible = __instance.GetComponent<Accessible>();
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GoodStackAccessible), nameof(GoodStackAccessible.Enable))]
    public static bool PatchGoodStackAccessible(GoodStackAccessible __instance)
    {
        if (!MSettings.ExtendDemolishValue || !__instance.HasComponent<Demolishable>()) { return true; }

        var generator = __instance.GetComponent<BlockObjectAccessGenerator>();

        var bo = __instance._blockObjectCenter._blockObject;
        var maxHeight = bo._blockService._mapSize.TotalSize.z;
        var z = bo.CoordinatesAtBaseZ.z;

        var minZ = ModUtils.GetMinZ(z);
        var maxZ = ModUtils.GetMaxZ(z, maxHeight);

        __instance._accessible.SetAccesses(generator.GenerateAccesses(minZ, maxZ));

        return false;
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(DemolishBehavior), nameof(DemolishBehavior.Decide))]
    public static IEnumerable<CodeInstruction> ChangeDecideToWalk(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo expectingMethod = typeof(Demolisher).PropertyGetter(nameof(Demolisher.Demolishable));

        foreach (var ins in instructions)
        {
            if (!ins.Calls(expectingMethod))
            {
                yield return ins;
                continue;
            }

            // Still push it into the stack, then call WalkToDemolishable
            yield return ins;
            yield return new(OpCodes.Ldarg_1); // Load agent
            yield return new(OpCodes.Ldloc_0); // Load demolisher
            yield return CodeInstruction.Call(() => WalkToDemolishable);

            // Return the result
            yield return new(OpCodes.Ret);
            yield break;
        }

        // Should not reach here
        throw new InvalidOperationException("Failed to patch DemolishBehavior.Decide");
    }

    static Decision WalkToDemolishable(Demolishable demolishable, BehaviorAgent agent, Demolisher demolisher)
    {
        if (MSettings.ExtendDemolishValue)
        {            
            var goodStackAccessible = demolishable.GetComponent<GoodStackAccessible>();
            if (!goodStackAccessible)
            {
                goto ON_FAILURE;
            }
            else
            {
                var walker = agent.GetComponent<WalkToAccessibleExecutor>();
                var status = walker.Launch(goodStackAccessible._accessible);

                switch (status)
                {
                    case ExecutorStatus.Success:
                        goto ON_SUCCESS;
                    case ExecutorStatus.Failure:
                        goto ON_FAILURE;
                    case ExecutorStatus.Running:
                        return Decision.ReturnWhenFinished(walker);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        else
        {
            // Use original code
            WalkToReservableExecutor component2 = agent.GetComponent<WalkToReservableExecutor>();
            DemolishableReacher component3 = demolishable.GetComponent<DemolishableReacher>();

            var status = component2.Launch(component3);
            switch (status)
            {
                case ExecutorStatus.Success:
                    goto ON_SUCCESS;
                case ExecutorStatus.Failure:
                    goto ON_FAILURE;
                case ExecutorStatus.Running:
                    return Decision.ReturnWhenFinished(component2);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    ON_SUCCESS:
        return DemolishBehavior.Demolish(agent);
    ON_FAILURE:
        return DemolishBehavior.UnreserveDemolishable(demolisher);
    }

}
