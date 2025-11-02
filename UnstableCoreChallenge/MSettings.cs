namespace UnstableCoreChallenge;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    public override string ModId { get; } = nameof(UnstableCoreChallenge);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    static readonly ImmutableArray<string> Difficulties = ["Easy", "Normal", "Hard"];

    public LimitedStringModSetting Difficulty { get; } = new(
        1,
        [..Difficulties.Select(q => new LimitedStringModSettingValue(q, "LV.USC.Diff" + q))],
        ModSettingDescriptor
            .CreateLocalized("LV.USC.Difficulty")
            .SetLocalizedTooltip("LV.USC.DifficultyDesc")
    );
    public int DifficultyValue => Difficulties.IndexOf(Difficulty.Value);

    public ModSetting<bool> EndlessMode { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.USC.EndlessMode")
        .SetLocalizedTooltip("LV.USC.EndlessModeDesc")
    );

}
