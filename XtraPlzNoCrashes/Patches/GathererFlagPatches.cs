namespace XtraPlzNoCrashes.Patches;

[HarmonyPatch(typeof(GathererFlag))]
public static class GathererFlagPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(GathererFlag.InitializeEntity))]
    public static void DistinctGatherables(GathererFlag __instance)
    {
        __instance.AllowedGatherables = [..__instance.AllowedGatherables
            .Distinct(GatherableEqualizer.Instance)];
    }

    class GatherableEqualizer : IEqualityComparer<GatherableSpec>
    {

        public static readonly GatherableEqualizer Instance = new();

        public bool Equals(GatherableSpec x, GatherableSpec y) => x.Yielder.Yield.Id.Equals(y.Yielder.Yield.Id);

        public int GetHashCode(GatherableSpec obj) => obj.Yielder.Yield.Id.GetHashCode();

    }

}
