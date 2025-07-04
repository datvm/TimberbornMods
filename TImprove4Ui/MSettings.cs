﻿namespace TImprove4Ui;

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
    static readonly Color DefaultHighlightColor = new(61 / 255f, 100 / 255f, 255 / 255f);

    public static bool ScrollableEntityPanel { get; private set; }
    public static bool RemovePathHighlight { get; private set; }
    public static bool AutoExpandCounter { get; private set; }
    public static string ToolDescPos { get; private set; } = ToolDescPositions[1].Value;
    public static bool NeverHideCounter { get; private set; }

    public override string ModId { get; } = nameof(TImprove4Ui);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    readonly ModSetting<bool> scrollableEntityPanel = CreateBoolModSettings("LV.T4UI.ScrollableEntityPanel", true);
    readonly ModSetting<bool> removePathHighlight = CreateBoolModSettings("LV.T4UI.RemovePathHighlight");
    readonly ModSetting<bool> autoExpandCounter = CreateBoolModSettings("LV.T4UI.AutoExpandCounter");
    readonly ModSetting<bool> neverHideCounter = CreateBoolModSettings("LV.T4UI.NeverHideCounter");

    public ColorModSetting HighlightSimilarColor { get; } = new(DefaultHighlightColor, ModSettingDescriptor
        .CreateLocalized("LV.T4UI.HighlightSimilarColor")
        .SetLocalizedTooltip("LV.T4UI.HighlightSimilarColorDesc"), false);
    public ModSetting<bool> HighlightSimilar { get; } = CreateBoolModSettings("LV.T4UI.HighlightSimilar");
    public ModSetting<bool> HighlightTubeway { get; } = CreateBoolModSettings("LV.T4UI.HighlightTubeway");
    public ModSetting<bool> HighlightStorage { get; } = CreateBoolModSettings("LV.T4UI.HighlightStorage");
    public ModSetting<bool> DisableShadows { get; } = CreateBoolModSettings("LV.T4UI.DisableShadows");
    public ModSetting<bool> AddNegativeNeeds { get; } = CreateBoolModSettings("LV.T4UI.AddNegativeNeeds", true);

    public readonly LimitedStringModSetting toolDescPos = new(
        1,
        ToolDescPositions,
        ModSettingDescriptor
            .CreateLocalized("LV.T4UI.ToolDescPos")
            .SetLocalizedTooltip("LV.T4UI.ToolDescPosDesc")
    );

    public override void OnBeforeLoad()
    {
        ModSetting[] menuOnly = [scrollableEntityPanel];

        foreach (var s in menuOnly)
        {
            s.Descriptor.SetEnableCondition(() => contextProvider.Context == ModSettingsContext.MainMenu);
        }

        AddCustomModSetting(scrollableEntityPanel, nameof(scrollableEntityPanel));
        AddCustomModSetting(removePathHighlight, nameof(removePathHighlight));
        AddCustomModSetting(toolDescPos, nameof(toolDescPos));
        AddCustomModSetting(autoExpandCounter, nameof(autoExpandCounter));
        AddCustomModSetting(neverHideCounter, nameof(neverHideCounter));
    }

    public override void OnAfterLoad()
    {
        ModSettingChanged += (_, _) => UpdateValues();
        UpdateValues();
    }

    void UpdateValues()
    {
        ScrollableEntityPanel = scrollableEntityPanel.Value;
        RemovePathHighlight = removePathHighlight.Value;
        ToolDescPos = toolDescPos.Value;
        AutoExpandCounter = autoExpandCounter.Value;
        NeverHideCounter = neverHideCounter.Value;
    }

    static ModSetting<bool> CreateBoolModSettings(string loc, bool defaultValue = false) =>
        new(
            defaultValue,
            ModSettingDescriptor
                .CreateLocalized(loc)
                .SetLocalizedTooltip(loc + "Desc")
        );

}
