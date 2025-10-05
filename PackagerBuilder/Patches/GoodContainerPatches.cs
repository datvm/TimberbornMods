namespace PackagerBuilder.Patches;

[HarmonyPatch]
public static class GoodContainerPatches
{

    [HarmonyPostfix, HarmonyPatch(typeof(GoodService), nameof(GoodService.LoadGoods))]
    public static void ReplaceGoodMaterial(GoodService __instance)
    {
        List<GoodSpec> replacing = [];

        foreach (var (k, g) in __instance._goodSpecsById)
        {
            if (GoodBuilder.IsPackagedGood(k)
                || !g.ContainerMaterial) { continue; }

            var packagedGoodId = GoodBuilder.GetPackagedGoodId(k);
            if (!__instance._goodSpecsById.TryGetValue(packagedGoodId, out var pgk)) { continue; }

            replacing.Add(pgk with
            {
                ContainerColor = g.ContainerColor,
                ContainerMaterial = g.ContainerMaterial,
            });
        }

        if (replacing.Count == 0) { return; }
        foreach (var g in replacing)
        {
            __instance._goodSpecsById[g.Id] = g;
        }
    }

}
