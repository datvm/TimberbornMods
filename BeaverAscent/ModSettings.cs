
using Timberborn.MapStateSystem;

namespace BeaverAscent;
public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    static readonly FieldInfo MaxMapEditorTerrainHeightField = typeof(MapSize).GetField(nameof(MapSize.MaxMapEditorTerrainHeight), BindingFlags.Public | BindingFlags.Static);
    static readonly FieldInfo MaxGameTerrainHeightField = typeof(MapSize).GetField(nameof(MapSize.MaxGameTerrainHeight), BindingFlags.Public | BindingFlags.Static);
    static readonly FieldInfo MaxMapEditorAboveTerrainHeightField = typeof(MapSize).GetField("MaxHeightAboveTerrain", BindingFlags.NonPublic | BindingFlags.Static);

    public override string ModId => nameof(BeaverAscent);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    ModSetting<bool>? changeHeight, allowEditorUpToTerrain, freeCamera;
    RangeIntModSetting? terrainHeight, aboveTerrainHeight;

    public bool FreeCamera => freeCamera?.Value == true;
    public event Action<bool> FreeCameraChanged = delegate { };

    public override void OnAfterLoad()
    {
        freeCamera = new ModSetting<bool>(
            false,
            ModSettingDescriptor
            .CreateLocalized("LV.BAsc.FreeCamera")
            .SetLocalizedTooltip("LV.BAsc.FreeCameraDesc"));

        changeHeight = new ModSetting<bool>(
            false,
            ModSettingDescriptor
                .CreateLocalized("LV.BAsc.ChangeHeight")
                .SetLocalizedTooltip("LV.BAsc.ChangeHeightDesc"));

        terrainHeight = new RangeIntModSetting(
            22,
            22,
            100,
            ModSettingDescriptor
                .CreateLocalized("LV.BAsc.TerrainHeight")
                .SetLocalizedTooltip("LV.BAsc.TerrainHeightDesc")
                .SetEnableCondition(() => changeHeight.Value));

        aboveTerrainHeight = new RangeIntModSetting(
            10,
            10,
            100,
            ModSettingDescriptor
                .CreateLocalized("LV.BAsc.AboveTerrainHeight")
                .SetLocalizedTooltip("LV.BAsc.AboveTerrainHeightDesc")
                .SetEnableCondition(() => changeHeight.Value));

        allowEditorUpToTerrain = new ModSetting<bool>(
            false,
            ModSettingDescriptor
                .CreateLocalized("LV.BAsc.AllowEditorUpToTerrainLimit")
                .SetLocalizedTooltip("LV.BAsc.AllowEditorUpToTerrainLimitDesc")
                .SetEnableCondition(() => changeHeight.Value));

        AddCustomModSetting(freeCamera, nameof(freeCamera));
        AddCustomModSetting(changeHeight, nameof(changeHeight));
        AddCustomModSetting(terrainHeight, nameof(terrainHeight));
        AddCustomModSetting(aboveTerrainHeight, nameof(aboveTerrainHeight));
        AddCustomModSetting(allowEditorUpToTerrain, nameof(allowEditorUpToTerrain));

        changeHeight.ValueChanged += (_, _) => UpdateValues();
        allowEditorUpToTerrain.ValueChanged += (_, _) => UpdateValues();
        terrainHeight.ValueChanged += (_, _) => UpdateValues();
        aboveTerrainHeight.ValueChanged += (_, _) => UpdateValues();

        freeCamera.ValueChanged += (_, e) => FreeCameraChanged(e);

        UpdateValues();
        FreeCameraChanged(FreeCamera);
    }

    void UpdateValues()
    {
        if (freeCamera is null || changeHeight is null || allowEditorUpToTerrain is null || terrainHeight is null || aboveTerrainHeight is null) { return; }

        var changing = changeHeight.Value;
        if (!changing) { return; }

        MaxGameTerrainHeightField.SetValue(null, terrainHeight.Value);
        MaxMapEditorTerrainHeightField.SetValue(null, terrainHeight.Value - (allowEditorUpToTerrain.Value ? 0 : 6));
        MaxMapEditorAboveTerrainHeightField.SetValue(null, aboveTerrainHeight.Value);
    }

}