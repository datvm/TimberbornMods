namespace ModdableTimberborn.SoakEffect
{
    public class ModdableSoakEffectConfig : IModdableTimberbornRegistryWithPatchConfig
    {
        public const string PatchCategoryName = $"{nameof(ModdableTimberborn)}.{nameof(SoakEffect)}";
        public static readonly ModdableSoakEffectConfig Instance = new();

        public string PatchCategory { get; } = PatchCategoryName;

        private ModdableSoakEffectConfig() { }

        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            if (!context.HasFlag(ConfigurationContext.Game)) { return; }

            configurator
                .BindTemplateModule(h => h
                    .AddDecorator<Character, ModdableSoakEffectComponent>()
                )
            ;
        }
    }
}

namespace ModdableTimberborn.Registry
{
    public partial class ModdableTimberbornRegistry
    {
        public bool SoakEffectUsed { get; private set; }

        /// <summary>
        /// Add <see cref="ModdableSoakEffectComponent"/> for all <see cref="Character"/>s for efficient water exposure tracking.
        /// </summary>
        public ModdableTimberbornRegistry UseSoakEffect()
        {
            if (SoakEffectUsed) { return this; }

            SoakEffectUsed = true;
            AddConfigurator(ModdableSoakEffectConfig.Instance);

            return this;
        }
    }
}