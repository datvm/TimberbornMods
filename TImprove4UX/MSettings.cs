namespace TImprove4UX;

public class MSettings(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{

    public override string ModId { get; } = nameof(TImprove4UX);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    public ModSetting<bool> ShiftToDelete { get; } = new(true, ModSettingDescriptor
        .CreateLocalized("LV.T4UX.ShiftToDelete")
        .SetLocalizedTooltip("LV.T4UX.ShiftToDeleteDesc"));
    public ModSetting<bool> ShiftToDeleteAll { get; private set; } = null!;

    public override void OnBeforeLoad()
    {
        base.OnBeforeLoad();

        ShiftToDeleteAll = new(false, ModSettingDescriptor
            .Create("  " + t.T("LV.T4UX.ShiftToDeleteAll"))
            .SetLocalizedTooltip("LV.T4UX.ShiftToDeleteAllDesc"));
    }

    public override void OnAfterLoad()
    {
        base.OnAfterLoad();

        ShiftToDeleteAll.Descriptor.SetEnableCondition(() => ShiftToDelete.Value);
    }

}
