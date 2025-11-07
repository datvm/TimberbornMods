namespace MultiYieldersPlzNoCrashes;

[HarmonyPatch]
public static class Patch
{

    [HarmonyPostfix, HarmonyPatch(typeof(GathererFlag), nameof(GathererFlag.InitializeEntity))]
    public static void DistinctGatherables(GathererFlag __instance)
    {
        __instance.AllowedGatherables = [..__instance.AllowedGatherables
            .Distinct(GatherableEqualizer.Instance)];
    }

    class GatherableEqualizer : IEqualityComparer<GatherableSpec>
    {

        public static readonly GatherableEqualizer Instance = new();

        public bool Equals(GatherableSpec x, GatherableSpec y) => x.YielderSpec.Yield.GoodId.Equals(y.YielderSpec.Yield.GoodId);

        public int GetHashCode(GatherableSpec obj) => obj.YielderSpec.Yield.GoodId.GetHashCode();

    }

}
