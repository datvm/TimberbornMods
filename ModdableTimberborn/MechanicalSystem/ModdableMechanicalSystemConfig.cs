namespace ModdableTimberborn.MechanicalSystem
{
    public class ModdableMechanicalSystemConfig : IModdableTimberbornRegistryWithPatchConfig
    {
        public const string PatchCategoryName = $"{nameof(ModdableTimberborn)}.{nameof(MechanicalSystem)}";
        public static readonly ModdableMechanicalSystemConfig Instance = new();

        ConfigurationContext IModdableTimberbornRegistryConfig.AvailableContexts => ConfigurationContext.Game;

        public string PatchCategory { get; } = PatchCategoryName;

        ModdableMechanicalSystemConfig() { }


        public void Configure(Configurator configurator, ConfigurationContext context)
        {
            configurator
                .BindTemplateModule(h => h
                    .AddDecorator<MechanicalNode, ModdableMechanicalNode>()
                )
            ;
        }

    }
}

namespace ModdableTimberborn.Registry
{
    public partial class ModdableTimberbornRegistry
    {
        public bool MechanicalSystemUsed { get; private set; }
        public ModdableTimberbornRegistry UseMechanicalSystem()
        {
            if (MechanicalSystemUsed) { return this; }

            MechanicalSystemUsed = true;
            AddConfigurator(ModdableMechanicalSystemConfig.Instance);

            return this;
        }
    }
}