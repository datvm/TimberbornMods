namespace TImprove4UX;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t,
    IModSettingsContextProvider contextProvider
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(TImprove4UX);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    public ModSetting<bool> ShiftToDelete { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.T4UX.ShiftToDelete")
        .SetLocalizedTooltip("LV.T4UX.ShiftToDeleteDesc"));
    public ModSetting<bool> ShiftToDeleteAll { get; private set; } = null!;

    public ModSetting<int> UndoCount { get; } = new(5, ModSettingDescriptor
        .CreateLocalized("LV.T4UX.UndoCount")
        .SetLocalizedTooltip("LV.T4UX.UndoCountDesc"));

    public ModSetting<bool> CollapseEntityPanel { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.T4UX.CollapseEntityPanel")
        .SetLocalizedTooltip("LV.T4UX.CollapseEntityPanelDesc"));
    public ModSetting<bool> CollapseEntityPanelGlobal { get; } = new(false, ModSettingDescriptor
        .CreateLocalized("LV.T4UX.CollapseEntityPanelGlobal")
        .SetLocalizedTooltip("LV.T4UX.CollapseEntityPanelGlobalDesc"));

    public ModSetting<bool> ShowDynamiteDestruction { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.T4UX.ShowDynamiteDestruction")
        .SetLocalizedTooltip("LV.T4UX.ShowDynamiteDestructionDesc"));

    public override void OnBeforeLoad()
    {
        base.OnBeforeLoad();

        ShiftToDeleteAll = new(false, ModSettingDescriptor
            .Create("  " + t.T("LV.T4UX.ShiftToDeleteAll"))
            .SetLocalizedTooltip("LV.T4UX.ShiftToDeleteAllDesc"));

        var isMainMenu = contextProvider.Context == ModSettingsContext.MainMenu;
        CollapseEntityPanel.Descriptor.SetEnableCondition(() => isMainMenu);
        CollapseEntityPanelGlobal.Descriptor.SetEnableCondition(() => CollapseEntityPanel.Value);

        ShowDynamiteDestruction.Descriptor.SetEnableCondition(() => !MStarter.HasDirectionalDynamite);
    }

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();

        ShiftToDeleteAll.Descriptor.SetEnableCondition(() => ShiftToDelete.Value);
    }

}
