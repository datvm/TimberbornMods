namespace TImprove4Ui.Patches;

[HarmonyPatch(typeof(ProductionItemFactory))]
public static class ProductionItemFactoryPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(ProductionItemFactory.CreateInput), [typeof(IEnumerable<VisualElement>)])]
    public static void WrapInput(VisualElement __result) => WrapIO(__result);

    static void WrapIO(VisualElement e)
    {
        var el = e.Q(className: "production-item__items--input");
        if (el is null) { return; }

        el.style.flexWrap = Wrap.Wrap;
    }

}
