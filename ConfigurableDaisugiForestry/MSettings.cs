namespace ConfigurableDaisugiForestry;

public class MSettings(ISettings settings, ModSettingsOwnerRegistry modSettingsOwnerRegistry, ModRepository modRepository) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    static readonly MSettingTemplate Default = MSettingTemplate.Normal;

    public override string ModId { get; } = nameof(ConfigurableDaisugiForestry);
    public override ModSettingsContext ChangeableOn => ModSettingsContext.All;

    public MSettingDifficultySetting Templates { get; } = new();

    public ModSetting<int> BirchDays { get; } = Create(Default.Birch.Days, nameof(BirchDays));
    public ModSetting<int> BirchLogs { get; } = Create(Default.Birch.Logs, nameof(BirchLogs));
    public ModSetting<int> BirchHarvest { get; } = Create(Default.Birch.HarvestHours,nameof(BirchHarvest));
    public ModSetting<bool> BirchPlank { get; } = Create(Default.Birch.IsPlank, nameof(BirchPlank));

    public ModSetting<int> OakDays { get; } = Create(Default.Oak.Days, nameof(OakDays));
    public ModSetting<int> OakLogs { get; } = Create(Default.Oak.Logs, nameof(OakLogs));
    public ModSetting<int> OakHarvest { get; } = Create(Default.Oak.HarvestHours, nameof(OakHarvest));
    public ModSetting<bool> OakPlank { get; } = Create(Default.Oak.IsPlank, nameof(OakPlank));

    public DaisugiValues BirchValues => new(BirchDays.Value, BirchLogs.Value, BirchHarvest.Value, BirchPlank.Value);
    public DaisugiValues OakValues => new(OakDays.Value, OakLogs.Value, OakHarvest.Value, OakPlank.Value);

    static ModSetting<int> Create(int defaultValue, string key) => new(defaultValue, ModSettingDescriptor
        .CreateLocalized("LV.DF." + key)
        .SetLocalizedTooltip("LV.DF." + key + "Desc"));
    static ModSetting<bool> Create(bool defaultValue, string key) => new(defaultValue, ModSettingDescriptor
        .CreateLocalized("LV.DF." + key)
        .SetLocalizedTooltip("LV.DF." + key + "Desc"));

}
