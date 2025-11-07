namespace TestMod;

public class MStarter : IModStarter
{

    void IModStarter.StartMod(IModEnvironment modEnvironment)
    {
        var h = new Harmony(nameof(TestMod));
        h.PatchAll();
    }


}

[HarmonyPatch]
public static class TestPatch
{

    [HarmonyPrefix, HarmonyPatch(typeof(YieldRemovingBuilding), nameof(YieldRemovingBuilding.GetAllowedYielders))]
    public static void Prefix(YieldRemovingBuilding __instance)
    {
        __instance._yieldRemovingBuildingSpec = __instance.GetComponent<YieldRemovingBuildingSpec>();

        var gathererFlag = __instance.GetComponent<GathererFlag>();
        if (!gathererFlag) { return; }

        Debug.Log("Getting allowed yielders for " + __instance.Name);

        var yielders = __instance._templateService.GetAll<IYielderDecorable>();
        foreach (var yielder in yielders)
        {
            var spec = yielder.YielderSpec;

            if (__instance.IsAllowed(yielder.YielderSpec))
            {
                Debug.Log($"- {spec.Yield.Id} x{spec.Yield.Amount} from {spec.YielderComponentName}");
            }
        }

    }

}
