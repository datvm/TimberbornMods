namespace MacroManagement.Components.DummyComponents;

public class DummyWorkplace : Workplace, IDummyComponent<DummyWorkplace, Workplace>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
    Workplace original;
#nullable enable

    public new int NumberOfAssignedWorkers => 0;
    public new int MaxWorkers { get; private set; }

    public void Init(Workplace original)
    {
        this.original = original;
        MaxWorkers = original.MaxWorkers;
        DesiredWorkers = original.DesiredWorkers;

        _blockableBuilding = GetComponentFast<BlockableBuilding>();
        enabled = true;
    }

    public new void IncreaseDesiredWorkers()
    {
        this.Proxy(q => q.IncreaseDesiredWorkers());
        DesiredWorkers = original.DesiredWorkers;
    }

    public new void DecreaseDesiredWorkers()
    {
        this.Proxy(q => q.DecreaseDesiredWorkers());
        DesiredWorkers = original.DesiredWorkers;
    }
}

[HarmonyPatch(typeof(Workplace))]
public static class WorkplaceRedirect
{

    [HarmonyPrefix, HarmonyPatch(nameof(Workplace.Start))]
    public static bool BypassStart(Workplace __instance)
        => __instance.PatchBypass<DummyWorkplace, Workplace>();

    [HarmonyPrefix, HarmonyPatch(nameof(Workplace.NumberOfAssignedWorkers), MethodType.Getter)]
    public static bool RedirectNumberOfAssignedWorkers(Workplace __instance, ref int __result)
        => __instance.PatchRedirect<DummyWorkplace, Workplace, int>(q => q.NumberOfAssignedWorkers, ref __result);

    [HarmonyPrefix, HarmonyPatch(nameof(Workplace.MaxWorkers), MethodType.Getter)]
    public static bool RedirectMaxWorkers(Workplace __instance, ref int __result)
        => __instance.PatchRedirect<DummyWorkplace, Workplace, int>(q => q.MaxWorkers, ref __result);

    [HarmonyPrefix, HarmonyPatch(nameof(Workplace.IncreaseDesiredWorkers))]
    public static bool RedirectIncreaseDesiredWorkers(Workplace __instance)
        => __instance.PatchRedirect<DummyWorkplace, Workplace>(q => q.IncreaseDesiredWorkers());

    [HarmonyPrefix, HarmonyPatch(nameof(Workplace.DecreaseDesiredWorkers))]
    public static bool RedirectDecreaseDesiredWorkers(Workplace __instance)
        => __instance.PatchRedirect<DummyWorkplace, Workplace>(q => q.DecreaseDesiredWorkers());

}
