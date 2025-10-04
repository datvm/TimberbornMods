using ModdableTimberborn.DependencyInjection.PrefabGroup;
using ModdableTimberborn.DependencyInjection.Specs;

namespace ModdableTimberborn.DependencyInjection
{
    public class ModdableDependencyInjectionConfig : IModdableTimberbornRegistryWithPatchConfig
    {
        public const string PatchCategoryName = $"{nameof(ModdableTimberborn)}.{nameof(DependencyInjection)}";
        public static readonly ModdableDependencyInjectionConfig Instance = new();

        public string PatchCategory { get; } = PatchCategoryName;

        private ModdableDependencyInjectionConfig() { }

        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            configurator
                // Spec
                .MultiBindAndBindSingleton<IBlueprintModifierProvider, SpecServiceRunner>()
                .MultiBindSingleton<ISpecServiceTailRunner, SpecModifierService>()

                // Prefab
                .MultiBindAndBindSingleton<IPrefabGroupProvider, PrefabGroupServiceTailRunnerService>()
                .MultiBindSingleton<IPrefabGroupServiceTailRunner, PrefabModifierTailRunner>()
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
            AddConfigurator(ModdableDependencyInjectionConfig.Instance);

            return this;
        }
    }
}
