using ModSettings.Common;
using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SettingsSystem;

namespace ExtendedBuilderReach;
public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    protected override string ModId => nameof(ExtendedBuilderReach);

    static ModSetting<bool>? unlimitedAbove, unlimitedBelow;
    static RangeIntModSetting? rangeAbove, rangeBelow;

    public static bool UnlimitedAbove => unlimitedAbove?.Value == true;
    public static bool UnlimitedBelow => unlimitedBelow?.Value ?? true;
    public static int RangeAbove => rangeAbove?.Value ?? 1;
    public static int RangeBelow => rangeBelow?.Value ?? 1;

    protected override void OnAfterLoad()
    {
        unlimitedAbove = new(false, ModSettingDescriptor
            .CreateLocalized("EBR.UnlimitedAbove")
            .SetLocalizedTooltip("EBR.UnlimitedAboveDesc"));
        unlimitedBelow = new(true, ModSettingDescriptor
            .CreateLocalized("EBR.UnlimitedBelow")
            .SetLocalizedTooltip("EBR.UnlimitedBelowDesc"));

        rangeAbove = new(
            1, 0, 30,
            ModSettingDescriptor
                .CreateLocalized("EBR.RangeAbove")
                .SetLocalizedTooltip("EBR.RangeAboveDesc")
                .SetEnableCondition(() => unlimitedAbove?.Value == false));
        rangeBelow = new(
            1, 0, 30,
            ModSettingDescriptor
                .CreateLocalized("EBR.RangeBelow")
                .SetLocalizedTooltip("EBR.RangeBelowDesc")
                .SetEnableCondition(() => unlimitedBelow?.Value == false));

        AddCustomModSetting(unlimitedAbove, nameof(unlimitedAbove));
        AddCustomModSetting(unlimitedBelow, nameof(unlimitedBelow));
        AddCustomModSetting(rangeAbove, nameof(rangeAbove));
        AddCustomModSetting(rangeBelow, nameof(rangeBelow));
    }

}
