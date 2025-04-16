global using ConfigurableFaction.Services;
global using Timberborn.BlueprintSystem;
global using Timberborn.CoreUI;

namespace ConfigurableFaction;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    FactionBuildingService factionBuildings,
    ILoc t,
    DialogBoxShower diagShower
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public static bool TryRemovingDuplicates { get; private set; } = true;
    public static bool AddPlants { get; private set; } = false;

    public override string ModId { get; } = nameof(ConfigurableFaction);

    readonly ModSetting<bool> tryRemovingDuplicates = new(true,
        ModSettingDescriptor.CreateLocalized("LV.CFac.RemoveDup")
            .SetLocalizedTooltip("LV.CFac.RemoveDupDesc"));
    readonly ModSetting<bool> addPlants = new(false,
        ModSettingDescriptor.CreateLocalized("LV.CFac.AddPlants")
            .SetLocalizedTooltip("LV.CFac.AddPlantsDesc"));

    public override void OnAfterLoad()
    {
        AddCustomModSetting(tryRemovingDuplicates, nameof(tryRemovingDuplicates));
        
        AddCustomModSetting(addPlants, nameof(addPlants));
        addPlants.ValueChanged += (_, v) => ShowWarningMessage(v);
        
        AddFactionsSettings();

        UpdateValues();
    }

    void AddFactionsSettings()
    {
        foreach (var info in factionBuildings.Factions.Values)
        {
            AddFactionSettings(info);
        }
    }

    void AddFactionSettings(FactionInfo info)
    {
        var facId = info.Id;
        bool skipDuplicate = tryRemovingDuplicates.Value;

        var factionS = new ModSetting<bool>(
            false,
            ModSettingDescriptor.Create(info.Faction.DisplayName.Value));
        AddCustomModSetting(factionS, GetFactionKey(facId));

        factionS.ValueChanged += (_, e) => OnFactionSettingChanged(e, info);
        OnFactionSettingChanged(factionS.Value, info);

        foreach (var b in info.Buildings)
        {
            if (skipDuplicate && b.IsCommon) { continue; }

            var bId = b.Id;

            var bS = new ModSetting<bool>(
                false,
                ModSettingDescriptor.Create("  " + t.T(b.NameKey))
                    .SetEnableCondition(() => factionS.Value));
            AddCustomModSetting(bS, GetBuildingKey(facId, bId));

            bS.ValueChanged += (_, e) => OnBuildingSettingChanged(e, b);
            OnBuildingSettingChanged(bS.Value, b);
        }
    }

    void OnFactionSettingChanged(bool enabled, FactionInfo faction)
    {
        faction.Enabled = enabled;
    }

    void OnBuildingSettingChanged(bool enabled, SimpleBuildingSpec building)
    {
        building.Enabled = enabled;
    }

    public void Unload()
    {
        UpdateValues();
    }

    void UpdateValues()
    {
        TryRemovingDuplicates = tryRemovingDuplicates.Value;
        AddPlants = addPlants.Value;
    }

    void ShowWarningMessage(bool enabled)
    {
        if (!enabled) { return; }

        diagShower.Create()
            .SetMessage(t.T("LV.CFac.AddPlantsNotif"))
            .Show();
    }

    string GetFactionKey(string id) => "Faction." + id;
    string GetBuildingKey(string faction, string id) => $"Building.{faction}.{id}";

}
