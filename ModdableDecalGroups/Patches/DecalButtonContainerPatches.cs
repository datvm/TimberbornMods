
namespace ModdableDecalGroups.Patches;

[HarmonyPatch(typeof(DecalButtonContainer))]
public class DecalButtonContainerPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(DecalButtonContainer.Show))]
    public static bool Rearrange(DecalSupplier decalSupplier, DecalButtonContainer __instance)
    {
        DecalButtonContainerGroupper.Instance?.Show(__instance, decalSupplier);
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(nameof(DecalButtonContainer.RemoveButtons))]
    public static bool Remove(DecalButtonContainer __instance)
    {
        DecalButtonContainerGroupper.Instance?.RemoveButtons(__instance);
        return false;
    }

}
