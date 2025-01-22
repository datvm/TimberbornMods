using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SettingsSystem;
using Timberborn.SingletonSystem;

namespace ToggleAllMods;
internal class ModSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    readonly ModRepository modRepository = modRepository;

    ModSetting<bool>? enableAll, disableAll;
    ModSetting<string>? keepEnabled;

    protected override string ModId => nameof(ToggleAllMods);

    protected override void OnAfterLoad()
    {
        enableAll = new(
            false,
            ModSettingDescriptor.Create("Enable All")
                .SetTooltip("Check to enable all mods upon exit. Restart required and this is automatically unchecked.")
                .SetEnableCondition(() => disableAll?.Value == false)
        );
        disableAll = new(
            false,
            ModSettingDescriptor.Create("Disable All")
                .SetTooltip("Check to disable all mods upon exit. Restart required and this is automatically unchecked.")
                .SetEnableCondition(() => enableAll?.Value == false)
        );
        keepEnabled = new(
            "eMka.ModSettings;Harmony;ToggleAllMods",
            ModSettingDescriptor.Create("Keep Enabled")
                .SetTooltip("Semi-colon ';' separated list of mod IDs to keep enabled if Disable All is checked.")
        );

        AddCustomModSetting(enableAll, nameof(enableAll));
        AddCustomModSetting(disableAll, nameof(disableAll));
        AddCustomModSetting(keepEnabled, nameof(keepEnabled));
    }

    public void Unload()
    {
        if (enableAll?.Value == true)
        {
            ToggleAllMods(true);
            enableAll.SetValue(false);
        }
        else if (disableAll?.Value == true)
        {
            ToggleAllMods(false);
            disableAll.SetValue(false);
        }
    }

    void ToggleAllMods(bool enabled)
    {
        HashSet<string> ignoredList = [];
        if (!enabled)
        {
            ignoredList = new(keepEnabled?.Value.Split(';') ?? []);
        }

        foreach (var mod in modRepository.Mods)
        {
            if (!enabled && ignoredList.Contains(mod.Manifest.Id)) { continue; }
            ModPlayerPrefsHelper.ToggleMod(enabled, mod);
        }
    }

}
