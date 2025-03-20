namespace ColorfulBeavers;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository), IUnloadableSingleton
{
    public static bool AssignRandom { get; private set; } = true;
    public static bool AssignNamed { get; private set; } = true;
    public static bool ApplyToBot { get; private set; } = true;

    public override string ModId { get; } = nameof(ColorfulBeavers);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    readonly ModSetting<bool> assignRandom = new(
        true,
        ModSettingDescriptor.CreateLocalized("LV.CBv.AssignRandom")
            .SetLocalizedTooltip("LV.CBv.AssignRandomDesc")
    );

    readonly ModSetting<bool> assignNamed = new(
        true,
        ModSettingDescriptor.CreateLocalized("LV.CBv.AssignNamed")
            .SetLocalizedTooltip("LV.CBv.AssignNamedDesc")
    );

    readonly ModSetting<bool> applyToBot = new(
        true,
        ModSettingDescriptor.CreateLocalized("LV.CBv.ApplyToBot")
            .SetLocalizedTooltip("LV.CBv.ApplyToBotDesc")
    );

    public override void OnAfterLoad()
    {
        AddCustomModSetting(assignNamed, nameof(assignNamed));
        AddCustomModSetting(assignRandom, nameof(assignRandom));
        AddCustomModSetting(applyToBot, nameof(applyToBot));

        assignNamed.ValueChanged += (_, _) => UpdateValues();
        assignRandom.ValueChanged += (_, _) => UpdateValues();
        applyToBot.ValueChanged += (_, _) => UpdateValues();

        UpdateValues();
    }

    void UpdateValues()
    {
        AssignRandom = assignRandom.Value;
        AssignNamed = assignNamed.Value;
        ApplyToBot = applyToBot.Value;
    }

    public void Unload()
    {
        UpdateValues();
    }
}
