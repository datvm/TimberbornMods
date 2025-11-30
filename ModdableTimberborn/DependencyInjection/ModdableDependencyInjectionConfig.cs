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

        [Obsolete("You likely just need to add IWithDIConfig interface to your Config class instead.")]
        public ModdableTimberbornRegistry UseDependencyInjection()
        {
            InternalUseDependencyInjection();
            return this;
        }

        internal void InternalUseDependencyInjection()
        {
            if (DependencyInjectionUsed) { return; }

            DependencyInjectionUsed = true;
            AddConfigurator<ModdableDependencyInjectionConfig>();
        }

    }
}
