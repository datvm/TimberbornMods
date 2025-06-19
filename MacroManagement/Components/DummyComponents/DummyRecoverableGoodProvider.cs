namespace MacroManagement.Components.DummyComponents;

public class DummyRecoverableGoodProvider : RecoverableGoodProvider, IDummyComponent<DummyRecoverableGoodProvider, RecoverableGoodProvider>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
#nullable enable

    public void Init(RecoverableGoodProvider original) { }

    public new void GetRecoverableGoods(RecoverableGoodRegistry recoverableGoodRegistry)
    {
        this.Proxy(q =>
        {
            _recoverableGoodMultipliers.Clear();
            q.GetComponentsFast(_recoverableGoodMultipliers);

            foreach (var inventory in q._inventories.AllInventories)
            {
                AddGoodsFromInventory(recoverableGoodRegistry, inventory);
            }
        }, true);
    }

}

[HarmonyPatch(typeof(RecoverableGoodProvider))]
public static class RecoverableGoodProviderRedirect
{
    [HarmonyPrefix, HarmonyPatch(nameof(RecoverableGoodProvider.Awake))]
    public static bool BypassAwake(RecoverableGoodProvider __instance)
        => __instance.PatchBypass<DummyRecoverableGoodProvider, RecoverableGoodProvider>();

    [HarmonyPrefix, HarmonyPatch(nameof(RecoverableGoodProvider.GetRecoverableGoods))]
    public static bool RedirectGetRecoverableGoods(RecoverableGoodProvider __instance, RecoverableGoodRegistry recoverableGoodRegistry)
        => __instance.PatchRedirect<DummyRecoverableGoodProvider, RecoverableGoodProvider>(q => q.GetRecoverableGoods(recoverableGoodRegistry));
}