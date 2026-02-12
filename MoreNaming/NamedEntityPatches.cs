namespace MoreNaming;

[HarmonyPatch(typeof(NamedEntity))]
public static class NamedEntityPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(NamedEntity.IsEditable), MethodType.Getter)]
    public static bool AlwaysEditable(ref bool __result)
    {
        __result = true;
        return false;
    }

}
