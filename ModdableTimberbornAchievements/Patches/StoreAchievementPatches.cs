namespace ModdableTimberbornAchievements.Patches;

[HarmonyPatch(typeof(ContainerDefinition))]
public static class StoreAchievementPatches
{

    [HarmonyPostfix, HarmonyPatch(nameof(ContainerDefinition.InstallAll))]
    public static void ReplaceStoreAchievements(ContainerDefinition __instance)
    {
        __instance.Install(new ReplaceStoreConfig());
    }

    class ReplaceStoreConfig : Configurator
    {
        public override void Configure()
        {
            if (!this.TryGetBound<IStoreAchievements>(out var storeType) || storeType is null) { return; }

            ModdableStoreAchievement.OriginalStoreAchievementType = storeType;
            this
                .RemoveBinding<IStoreAchievements>()
                .BindSingleton(storeType)
                .BindSingleton<IStoreAchievements, ModdableStoreAchievement>()
            ;

        }
    }

}
