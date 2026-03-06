namespace XtraPlzNoCrashes.Patches;

[HarmonyPatch]
public static class TemplateCollectionServicePatches
{
    [HarmonyTranspiler, HarmonyPatch(typeof(TemplateCollectionService), nameof(TemplateCollectionService.Load))]
    public static IEnumerable<CodeInstruction> DistinctLoad(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var ins in instructions)
        {
            yield return ins;
            if (ins.opcode == OpCodes.Call && ins.operand is MethodInfo mi && mi.Name == nameof(Enumerable.SelectMany))
            {
                yield return new CodeInstruction(
                    OpCodes.Call,
                    typeof(TemplateCollectionServicePatches)
                    .Method(nameof(DistinctAssets))
                );
            }
        }
    }

    static IEnumerable<AssetRef<BlueprintAsset>> DistinctAssets(IEnumerable<AssetRef<BlueprintAsset>> assetRefs) => 
        assetRefs.Distinct(BlueprintAssetRefEqualizer.Instance);

    class BlueprintAssetRefEqualizer : IEqualityComparer<AssetRef<BlueprintAsset>>
    {

        public static readonly BlueprintAssetRefEqualizer Instance = new();

        public bool Equals(AssetRef<BlueprintAsset> x, AssetRef<BlueprintAsset> y) => x.Path.ToLowerInvariant().Equals(y.Path.ToLowerInvariant());

        public int GetHashCode(AssetRef<BlueprintAsset> obj) => obj.Path.ToLowerInvariant().GetHashCode();

    }

}