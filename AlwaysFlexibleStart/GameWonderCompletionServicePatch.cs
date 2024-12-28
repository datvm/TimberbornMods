using HarmonyLib;
using Timberborn.GameWonderCompletion;

namespace AlwaysFlexibleStart;

[HarmonyPatch(typeof(GameWonderCompletionService), nameof(GameWonderCompletionService.IsWonderCompletedWithAnyFaction))]
public static class GameWonderCompletionServicePatch
{

    public static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }

}
