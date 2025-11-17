namespace ModdableTimberborn.DependencyInjection
{
    public class ModdableDependencyInjectionConfig : IModdableTimberbornRegistryWithPatchConfig
    {
        public const string PatchCategoryName = $"{nameof(ModdableTimberborn)}.{nameof(DependencyInjection)}";

        public string PatchCategory { get; } = PatchCategoryName;

        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            if (context.IsBootstrapperContext())
            {
                configurator.Bind<AssetRefService>().AsSingleton().AsExported();
                return;
            }

            configurator
                // Spec
                .MultiBindAndBindSingleton<IBlueprintModifierProvider, SpecServiceRunner>()
                .BindSpecTailRunner<SpecModifierService>()

                // Prefab
                .MultiBindAndBindSingleton<ITemplateCollectionIdProvider, TemplateCollectionTailRunnerService>()
                .BindTemplateTailRunner<TemplateModifierTailRunner>()
            ;
        }
    }
}

namespace ModdableTimberborn.Registry
{
    public partial class ModdableTimberbornRegistry
    {
        public bool DependencyInjectionUsed { get; private set; }

        public ModdableTimberbornRegistry UseDependencyInjection()
        {
            if (DependencyInjectionUsed) { return this; }

            DependencyInjectionUsed = true;
            AddConfigurator<ModdableDependencyInjectionConfig>();

            return this;
        }
    }
}
