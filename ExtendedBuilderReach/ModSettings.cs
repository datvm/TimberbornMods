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
            .Create("Unlimited Above")
            .SetTooltip("Builders can always build above themselves (Game default: no)"));
        unlimitedBelow = new(true, ModSettingDescriptor
            .Create("Unlimited Below")
            .SetTooltip("Builders can always build below themselves (Game default: yes)"));

        rangeAbove = new(
            1, 0, 30,
            ModSettingDescriptor
                .Create("Range Above")
                .SetTooltip("Builders can build up to this many tiles above themselves (Game default: 1)")
                .SetEnableCondition(() => unlimitedAbove?.Value == false));
        rangeBelow = new(
            1, 0, 30,
            ModSettingDescriptor
                .Create("Range Below")
                .SetTooltip("Builders can build up to this many tiles below themselves (Game default: inifinite)")
                .SetEnableCondition(() => unlimitedBelow?.Value == false));

        AddCustomModSetting(unlimitedAbove, nameof(unlimitedAbove));
        AddCustomModSetting(unlimitedBelow, nameof(unlimitedBelow));
        AddCustomModSetting(rangeAbove, nameof(rangeAbove));
        AddCustomModSetting(rangeBelow, nameof(rangeBelow));
    }

}