namespace ToggleAllMods;
public class ModSettings : ModSettingsOwner, IUnloadableSingleton
{
    public static ModSettings? Instance { get; private set; }

    readonly ModRepository modRepository;

    ModSetting<bool>? enableAll, disableAll;
    ModSetting<string>? keepEnabled;

    public HashSet<string> KeepEnabledIds => new(keepEnabled?.Value.Split(';') ?? []);

    public ModSettings(
        ISettings settings,
        ModSettingsOwnerRegistry modSettingsOwnerRegistry,
        ModRepository modRepository
    ) : base(settings, modSettingsOwnerRegistry, modRepository)
    {
        Instance = this;

        this.modRepository = modRepository;
    }

    public override string ModId => nameof(ToggleAllMods);

    public override void OnAfterLoad()
    {
        enableAll = new(
            false,
            ModSettingDescriptor
                .CreateLocalized("DAM.EnableAll")
                .SetLocalizedTooltip("DAM.EnableAllDesc")
                .SetEnableCondition(() => disableAll?.Value == false)
        );
        disableAll = new(
            false,
            ModSettingDescriptor
                .CreateLocalized("DAM.DisableAll")
                .SetLocalizedTooltip("DAM.DisableAllDesc")
                .SetEnableCondition(() => enableAll?.Value == false)
        );
        keepEnabled = new(
            "eMka.ModSettings;Harmony;ToggleAllMods",
            ModSettingDescriptor
                .CreateLocalized("DAM.KeepEnabled")
                .SetLocalizedTooltip("DAM.KeepEnabledDesc")
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

        Instance = null;
    }

    public void ToggleAllMods(bool enabled)
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
