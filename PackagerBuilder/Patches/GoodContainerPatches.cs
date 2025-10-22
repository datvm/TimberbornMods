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
            if (Package10Provider.IsPackagedGood(k)
                || !g.ContainerMaterial) { continue; }

            var packagedGoodId = Package10Provider.GetPackagedGoodId(k);
            if (!__instance._goodSpecsById.TryGetValue(packagedGoodId, out var pgk)) { continue; }

            replacing.Add(pgk with
            {
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
