namespace BenchmarkAndOptimizer.Services;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    OptimizerSettings optimizerSettings
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(BenchmarkAndOptimizer);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    public OptimizerModSetting Optimizer { get; } = new(optimizerSettings);

}
