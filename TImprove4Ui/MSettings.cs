namespace TImprove4Ui;

public class MSettings(
   ISettings settings,
   ModSettingsOwnerRegistry modSettingsOwnerRegistry,
   ModRepository modRepository,
   IModSettingsContextProvider contextProvider
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    public static readonly ImmutableArray<LimitedStringModSettingValue> ToolDescPositions = 
        [..new string[] { "Left", "Center", "Right" }
        .Select(q => new LimitedStringModSettingValue(q, "LV.T4UI.ToolDescPos" + q))];

    public static bool ScrollableEntityPanel { get; private set; }
    public static bool RemovePathHighlight { get; private set; }
    public static bool HighlightPowerNetwork { get; private set; }
    public static string ToolDescPos { get; private set; } = ToolDescPositions[1].Value;

    public override string ModId { get; } = nameof(TImprove4Ui);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    readonly ModSetting<bool> scrollableEntityPanel = CreateBoolModSettings("LV.T4UI.ScrollableEntityPanel", true);
    readonly ModSetting<bool> removePathHighlight = CreateBoolModSettings("LV.T4UI.RemovePathHighlight");
    readonly ModSetting<bool> highlightPowerNetwork = CreateBoolModSettings("LV.T4UI.HighlightPowerNetwork");

    public readonly LimitedStringModSetting toolDescPos = new(
        1,
        ToolDescPositions,
        ModSettingDescriptor
            .CreateLocalized("LV.T4UI.ToolDescPos")
            .SetLocalizedTooltip("LV.T4UI.ToolDescPosDesc")
    );

    public override void OnBeforeLoad()
    {
        scrollableEntityPanel.Descriptor.SetEnableCondition(() => contextProvider.Context == ModSettingsContext.MainMenu);
    }

    public override void OnAfterLoad()
    {
        AddCustomModSetting(scrollableEntityPanel, nameof(scrollableEntityPanel));
        AddCustomModSetting(removePathHighlight, nameof(removePathHighlight));
        AddCustomModSetting(highlightPowerNetwork, nameof(highlightPowerNetwork));
        AddCustomModSetting(toolDescPos, nameof(toolDescPos));

        ModSettingChanged += (_, _) => UpdateValues();
        UpdateValues();
    }

    void UpdateValues()
    {
        ScrollableEntityPanel = scrollableEntityPanel.Value;
        RemovePathHighlight = removePathHighlight.Value;
        HighlightPowerNetwork = highlightPowerNetwork.Value;
        ToolDescPos = toolDescPos.Value;
    }

    static ModSetting<bool> CreateBoolModSettings(string loc, bool defaultValue = false) =>
        new(
            defaultValue,
            ModSettingDescriptor
                .CreateLocalized(loc)
                .SetLocalizedTooltip(loc + "Desc")
        );

}
