namespace TImprove.Patches;

[HarmonyPatch(typeof(BuilderPrioritizable))]
public static class BuilderPrioritizablePatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(BuilderPrioritizable.SetPriority))]
    public static void OnPrioritySet(Priority priority)
    {
        BuildingPrioritizableService.Instance?.LastSetPriority = priority;
    }

}

[HarmonyPatch]
public static class BuilderPrioritizableConstructorPatch
{
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod() => typeof(BuilderPrioritizable).Constructor();

    [HarmonyPostfix]
    public static void SetDefaultPriority(BuilderPrioritizable __instance)
    {
        var ins = BuildingPrioritizableService.Instance;
        if (ins is null) { return; }

        __instance.Priority = ins.DefaultBuildingPriority;
    }
}