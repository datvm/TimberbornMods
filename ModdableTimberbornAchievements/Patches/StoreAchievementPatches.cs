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
            var def = (ContainerDefinition)_containerDefinition;
            var registry = (BindingBuilderRegistry)def._bindingBuilderRegistry;
            
            if (!registry._boundBindingBuilders.TryGetValue(typeof(IStoreAchievements), out var binding))
            {
                return;
            }

            // Don't use TimberUI's TryGetBound because it does not work for ToExisting<> services
            var provisionBinding = (ProvisionBinding)binding.GetType().Field(nameof(BindingBuilder<>._provisionBinding)).GetValue(binding);
            var storeType = provisionBinding.Type ?? provisionBinding.ExistingType;
            if (storeType is null)
            {
                return;
            }

            ModdableStoreAchievement.OriginalStoreAchievementType = storeType;

            this.TryBind(storeType)?.AsSingleton();
            this
                .RemoveBinding<IStoreAchievements>()
                .BindSingleton<IStoreAchievements, ModdableStoreAchievement>()
            ;

        }
    }

}
