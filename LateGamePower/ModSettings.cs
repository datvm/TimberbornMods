using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SettingsSystem;

namespace LateGamePower;
internal class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    protected override string ModId => nameof(LateGamePower);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.Game | ModSettingsContext.MainMenu;

    protected override void OnAfterLoad()
    {

    }

}
