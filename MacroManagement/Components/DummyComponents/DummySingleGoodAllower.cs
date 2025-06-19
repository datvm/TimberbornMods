namespace MacroManagement.Components.DummyComponents;

public class DummySingleGoodAllower : SingleGoodAllower, IWarningDummyComponent<DummySingleGoodAllower, SingleGoodAllower>
{
#nullable disable
    public MMComponent MMComponent { get; set; }
    public ILoc T { get; set; }
    public DialogBoxShower DiaglogBoxShower { get; set; }

    SingleGoodAllower original;

#nullable enable

    public void Init(SingleGoodAllower original)
    {
        this.original = original;
        _inventory = original._inventory;
        AllowedGood = original.AllowedGood;
        enabled = true;
    }

    public new void Allow(string goodId) =>
        this.Confirm("LV.MacM.StockpileWarning", () =>
        {
            this.Proxy(q => q.Allow(goodId));
            AllowedGood = original.AllowedGood;
        });

    public new void Disallow() =>
        this.Confirm("LV.MacM.StockpileWarning", () =>
        {
            this.Proxy(q => q.Disallow());
            AllowedGood = original.AllowedGood;
        });

    public new int AllowedAmount(string goodId) => original.AllowedAmount(goodId);

    [Inject]
    public void Inject(ILoc t, DialogBoxShower diag) => this.InjectWarningDummy(t, diag);
}

[HarmonyPatch(typeof(SingleGoodAllower))]
public static class SingleGoodAllowerRedirect
{
    [HarmonyPrefix, HarmonyPatch(nameof(SingleGoodAllower.Allow))]
    public static bool RedirectAllow(SingleGoodAllower __instance, string goodId)
        => __instance.PatchRedirect<DummySingleGoodAllower, SingleGoodAllower>(q => q.Allow(goodId));

    [HarmonyPrefix, HarmonyPatch(nameof(SingleGoodAllower.Disallow))]
    public static bool RedirectDisallow(SingleGoodAllower __instance)
        => __instance.PatchRedirect<DummySingleGoodAllower, SingleGoodAllower>(q => q.Disallow());

    [HarmonyPrefix, HarmonyPatch(nameof(SingleGoodAllower.AllowedAmount))]
    public static bool RedirectAllowedAmount(SingleGoodAllower __instance, string goodId, ref int __result)
        => __instance.PatchRedirect<DummySingleGoodAllower, SingleGoodAllower, int>(q => q.AllowedAmount(goodId), ref __result);
}
