﻿namespace FasterConfigurableStorages;

public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(FasterConfigurableStorages);

    static ModSetting<float>? storageCapacityMultiplier;

    public static float StorageCapacityMultiplier => storageCapacityMultiplier?.Value ?? 2;

    public override void OnAfterLoad()
    {
        storageCapacityMultiplier = new ModSetting<float>(
            2,
            ModSettingDescriptor
                .CreateLocalized("LV.FCS.CapacityMultipler")
                .SetLocalizedTooltip("LV.FCS.CapacityMultiplerDesc"));

        AddCustomModSetting(storageCapacityMultiplier, nameof(StorageCapacityMultiplier));
    }

}
