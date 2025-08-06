namespace MacroManagement.Components.DummyComponents;

public class DummyGoodObtainer : GoodObtainer, IDummyComponent<DummyGoodObtainer, GoodObtainer>
{

#nullable disable
    public MMComponent MMComponent { get; set; }
    GoodObtainer original;
#nullable enable

    public new bool GoodObtainingEnabled => original.GoodObtainingEnabled;

    public void Init(GoodObtainer original)
    {
        this.original = original;
    }

    public new void EnableGoodObtaining() => this.Proxy(q => q.EnableGoodObtaining());
    public new void DisableGoodObtaining() => this.Proxy(q => q.DisableGoodObtaining());

}

[HarmonyPatch(typeof(GoodObtainer))]
public static class GoodObtainerRedirect
{
    [HarmonyPrefix, HarmonyPatch(nameof(GoodObtainer.EnableGoodObtaining))]
    public static bool RedirectEnableGoodObtaining(GoodObtainer __instance)
        => __instance.PatchRedirect<DummyGoodObtainer, GoodObtainer>(q => q.EnableGoodObtaining());

    [HarmonyPrefix, HarmonyPatch(nameof(GoodObtainer.DisableGoodObtaining))]
    public static bool RedirectDisableGoodObtaining(GoodObtainer __instance)
        => __instance.PatchRedirect<DummyGoodObtainer, GoodObtainer>(q => q.DisableGoodObtaining());

    [HarmonyPrefix, HarmonyPatch(nameof(GoodObtainer.GoodObtainingEnabled), MethodType.Getter)]
    public static bool RedirectGoodObtainingEnabled(GoodObtainer __instance, ref bool __result)
        => __instance.PatchRedirect<DummyGoodObtainer, GoodObtainer, bool>(q => q.GoodObtainingEnabled, ref __result);
}

