using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SettingsSystem;

namespace CutAllTrees;

public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    protected override string ModId => nameof(CutAllTrees);

    ModSetting<bool>? enabled, alwaysEnabled;

    public bool Enabled => enabled?.Value == true;
    public bool AlwaysEnabled
    {
        get
        {
            return alwaysEnabled?.Value == true;
        }
        set
        {
            alwaysEnabled?.SetValue(value);
        }
    }

    protected override void OnAfterLoad()
    {
        enabled = new(true, ModSettingDescriptor
            .Create("Enabled")
            .SetTooltip("Mark all the map as 'cuttable' area when loading a save with this mod for the first time (including a new game)"));

        alwaysEnabled = new(false, ModSettingDescriptor
            .Create("Always Enabled")
            .SetTooltip("Mark all the map as 'cuttable' area when loading a save with this mod even if it was already activated before. Automatically uncheck once you load a save.")
            .SetEnableCondition(() => enabled?.Value == true));

        AddCustomModSetting(enabled, nameof(enabled));
        AddCustomModSetting(alwaysEnabled, nameof(alwaysEnabled));
    }

}
