namespace MacroManagement.Components.DummyComponents;

public class DummyEmptiable : Emptiable, IDummyComponent<DummyEmptiable, Emptiable>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
    Emptiable original;
#nullable enable

    public new bool IsMarkedForEmptying => original.IsMarkedForEmptying;

    public void Init(Emptiable original)
    {
        this.original = original;
    }

    public new void MarkForEmptyingWithStatus() => this.Proxy(q => q.MarkForEmptyingWithStatus());
    public new void MarkForEmptyingWithoutStatus() => this.Proxy(q => q.MarkForEmptyingWithoutStatus());
    public new void UnmarkForEmptying() => this.Proxy(q => q.UnmarkForEmptying());
}

[HarmonyPatch(typeof(Emptiable))]
public static class EmptiableRedirect
{
    [HarmonyPrefix, HarmonyPatch(nameof(Emptiable.Awake))]
    public static bool BypassAwake(Emptiable __instance)
        => __instance.PatchBypass<DummyEmptiable, Emptiable>();

    [HarmonyPrefix, HarmonyPatch(nameof(Emptiable.Start))]
    public static bool BypassStart(Emptiable __instance)
        => __instance.PatchBypass<DummyEmptiable, Emptiable>();

    [HarmonyPrefix, HarmonyPatch(nameof(Emptiable.MarkForEmptyingWithStatus))]
    public static bool RedirectMarkForEmptyingWithStatus(Emptiable __instance)
        => __instance.PatchRedirect<DummyEmptiable, Emptiable>(q => q.MarkForEmptyingWithStatus());

    [HarmonyPrefix, HarmonyPatch(nameof(Emptiable.MarkForEmptyingWithoutStatus))]
    public static bool RedirectMarkForEmptyingWithoutStatus(Emptiable __instance)
        => __instance.PatchRedirect<DummyEmptiable, Emptiable>(q => q.MarkForEmptyingWithoutStatus());

    [HarmonyPrefix, HarmonyPatch(nameof(Emptiable.UnmarkForEmptying))]
    public static bool RedirectUnmarkForEmptying(Emptiable __instance)
        => __instance.PatchRedirect<DummyEmptiable, Emptiable>(q => q.UnmarkForEmptying());

    [HarmonyPrefix, HarmonyPatch(nameof(Emptiable.IsMarkedForEmptying), MethodType.Getter)]
    public static bool RedirectIsMarkedForEmptying(Emptiable __instance, ref bool __result)
        => __instance.PatchRedirect<DummyEmptiable, Emptiable, bool>(q => q.IsMarkedForEmptying, ref __result);
}
