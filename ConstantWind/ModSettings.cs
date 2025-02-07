using Timberborn.WindSystem;

namespace ConstantWind;

public class ModSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{

    static readonly FieldInfo minWindStrength = typeof(WindService).GetField("MinWindStrength", BindingFlags.NonPublic | BindingFlags.Static);
    static readonly FieldInfo maxWindStrength = typeof(WindService).GetField("MaxWindStrength", BindingFlags.NonPublic | BindingFlags.Static);
    
    readonly RangeIntModSetting windStrength = new(50, 0, 100,
        ModSettingDescriptor.CreateLocalized("CW.WindStrength")
            .SetLocalizedTooltip("CW.WindStrengthDesc"));

    protected override string ModId => nameof(ConstantWind);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

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
