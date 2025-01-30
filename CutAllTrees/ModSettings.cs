using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SettingsSystem;

namespace CutAllTrees;

public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    protected override string ModId => "MarkAllTrees"; // Do not use nameof for this project
    
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
            .CreateLocalized("CAT.Enabled")
            .SetLocalizedTooltip("CAT.EnabledDesc"));

        alwaysEnabled = new(false, ModSettingDescriptor
            .CreateLocalized("CAT.AlwaysEnabled")
            .SetLocalizedTooltip("CAT.AlwaysEnabledDesc")
            .SetEnableCondition(() => enabled?.Value == true));

        AddCustomModSetting(enabled, nameof(enabled));
        AddCustomModSetting(alwaysEnabled, nameof(alwaysEnabled));
    }

}
