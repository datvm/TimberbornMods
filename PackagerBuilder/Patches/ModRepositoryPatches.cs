namespace PackagerBuilder.Patches;

[HarmonyPatch(typeof(ModRepository))]
public static class ModRepositoryPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(ModRepository.Load))]
    public static void AfterLoad(ModRepository __instance)
    {
        MConfig.HasPackagerMod ??= __instance.EnabledMods.Any(q => q.Manifest.Id == PackagerModBuilder.ModId);
    }

}
