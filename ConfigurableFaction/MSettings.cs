
namespace ConfigurableFaction;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    public override string ModId { get; } = nameof(ConfigurableFaction);

}
