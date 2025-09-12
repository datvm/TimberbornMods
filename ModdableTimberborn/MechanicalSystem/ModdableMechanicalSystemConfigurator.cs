namespace ModdableTimberborn.MechanicalSystem;

public class ModdableMechanicalSystemConfigurator : IModdableTimberbornRegistryWithPatchConfig
{
    public const string PatchCategoryName = $"{nameof(ModdableTimberborn)}.{nameof(MechanicalSystem)}";
    public static readonly ModdableMechanicalSystemConfigurator Instance = new();


    public string PatchCategory { get; } = PatchCategoryName;


    private ModdableMechanicalSystemConfigurator() { }


    public void Configure(Configurator configurator, ConfigurationContext context)
    {
        if (!context.HasFlag(ConfigurationContext.Game)) { return; }

        configurator
            .BindTemplateModule(h => h
                .AddDecorator<MechanicalNodeSpec, ModdableMechanicalNode>()
            )
        ;
    }

}
