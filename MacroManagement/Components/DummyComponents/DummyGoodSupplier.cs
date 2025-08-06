namespace MacroManagement.Components.DummyComponents;

public class DummyGoodSupplier : GoodSupplier, IDummyComponent<DummyGoodSupplier, GoodSupplier>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
    GoodSupplier original;
#nullable enable

    public new bool IsSupplying => original.IsSupplying;

    public void Init(GoodSupplier original)
    {
        this.original = original;
    }

    public new void EnableSupplying() => this.Proxy(q => q.EnableSupplying());
    public new void DisableSupplying() => this.Proxy(q => q.DisableSupplying());
}

[HarmonyPatch(typeof(GoodSupplier))]
public static class GoodSupplierRedirect
{
    [HarmonyPrefix, HarmonyPatch(nameof(GoodSupplier.EnableSupplying))]
    public static bool RedirectEnableSupplying(GoodSupplier __instance)
        => __instance.PatchRedirect<DummyGoodSupplier, GoodSupplier>(q => q.EnableSupplying());

    [HarmonyPrefix, HarmonyPatch(nameof(GoodSupplier.DisableSupplying))]
    public static bool RedirectDisableSupplying(GoodSupplier __instance)
        => __instance.PatchRedirect<DummyGoodSupplier, GoodSupplier>(q => q.DisableSupplying());

    [HarmonyPrefix, HarmonyPatch(nameof(GoodSupplier.IsSupplying), MethodType.Getter)]
    public static bool RedirectIsSupplying(GoodSupplier __instance, ref bool __result)
        => __instance.PatchRedirect<DummyGoodSupplier, GoodSupplier, bool>(q => q.IsSupplying, ref __result);
}
