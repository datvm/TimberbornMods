namespace UnstableCoreChallenge.Services;

[BindSingleton(Contexts = BindAttributeContext.MainMenu | BindAttributeContext.Game)]
public class MSettings(
    UnstableCoreDifficultySpecService diffs,
    ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) 
    : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId => nameof(UnstableCoreChallenge);

    LimitedStringModSetting diffSettings = null!;
    public UnstableCoreChallengeDifficultySpec SelectedDifficulty => diffs.GetDifficulty(diffSettings.Value);

    public override void OnBeforeLoad()
    {
        List<NonLocalizedLimitedStringModSettingValue> diffValues = [];

        var defaultIndex = 0;
        for (int i = 0; i < diffs.Difficulties.Length; i++)
        {
            var d = diffs.Difficulties[i];
            diffValues.Add(new(d.Name.Value));

            if (d.IsDefault)
            {
                defaultIndex = i;
            }
        }

        diffSettings = new(defaultIndex, diffValues, ModSettingDescriptor
            .CreateLocalized("LV.USC.Difficulty")
            .SetLocalizedTooltip("LV.USC.DifficultyDesc"));

        AddCustomModSetting(diffSettings, "Difficulty");
    }

}
