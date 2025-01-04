using ModSettings.Common;
using ModSettings.Core;
using System.Reflection;
using Timberborn.Modding;
using Timberborn.SettingsSystem;
using Timberborn.SingletonSystem;
using Timberborn.WindSystem;

namespace ConstantWind;

public class ModSettings : ModSettingsOwner, IUnloadableSingleton
{

    static readonly FieldInfo minWindStrength = typeof(WindService).GetField("MinWindStrength", BindingFlags.NonPublic | BindingFlags.Static);
    static readonly FieldInfo maxWindStrength = typeof(WindService).GetField("MaxWindStrength", BindingFlags.NonPublic | BindingFlags.Static);

    readonly RangeIntModSetting windStrength = new(50, 0, 100,
        ModSettingDescriptor.Create("Wind Strength")
            .SetTooltip("The constant wind strength, from 0% to 100%."));

    public ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : base(settings, modSettingsOwnerRegistry, modRepository)
    {
    }

    protected override string ModId => nameof(ConstantWind);

    protected override void OnAfterLoad()
    {
        AddCustomModSetting(windStrength, nameof(windStrength));

        windStrength.ValueChanged += (sender, e) => SetValue(windStrength.Value);
        SetValue(windStrength.Value);
    }

    public void Unload()
    {
        SetValue(windStrength.Value);
    }

    void SetValue(int value)
    {
        var actualValue = value / 100f;
        minWindStrength.SetValue(null, actualValue);
        maxWindStrength.SetValue(null, actualValue);
    }

}
