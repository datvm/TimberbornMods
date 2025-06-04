namespace TImprove.Settings;

public class MSettingsNewDay(
    ISettings settings,
    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
    ModRepository modRepository,
    ILoc t
) : ModSettingsOwner(settings, modSettingsOwnerRegistry, modRepository)
{
    static readonly ImmutableArray<NewDayActionValue> NewDayActions = TImproveUtils.GetEnumValues<NewDayActionValue>();
    static readonly string NoneValue = NewDayActionValue.None.ToString();

    public override string HeaderLocKey => "LV.TI.NewDayAction";

    public override string ModId { get; } = nameof(TImprove);
    public override ModSettingsContext ChangeableOn { get; } = ModSettingsContext.All;

    bool InternalImmediateHasAction => NewDayAction.Value != NoneValue;
    public bool HasAction => NewDayActionValue != NewDayActionValue.None;
    public bool ActionOnAllDays => NewDayAll.Value;

    public NewDayActionValue NewDayActionValue { get; private set; }
    public LimitedStringModSetting NewDayAction { get; } = TImproveUtils.CreateLimitedStringModSetting(NewDayActions, "LV.TI.NewDayAction");

    public ModSetting<bool> NewDayAll { get; private set; } = null!;
    public ModSetting<bool> NewDayWarning { get; private set; } = null!;
    public ModSetting<bool> NewDayHazard { get; private set; } = null!;
    public ModSetting<bool> NewDayNewCycle { get; private set; } = null!;

    public override void OnBeforeLoad()
    {
        NewDayAll = CreateNewDayCheck("All");
        NewDayWarning = CreateNewDayCheck("Warning");
        NewDayHazard = CreateNewDayCheck("Hazard", true);
        NewDayNewCycle = CreateNewDayCheck("NewCycle");

        SetCondition(NewDayAll, true);
        SetCondition(NewDayWarning);
        SetCondition(NewDayHazard);
        SetCondition(NewDayNewCycle);
    }

    public override void OnAfterLoad()
    {
        NewDayAction.ValueChanged += NewDayAction_ValueChanged;
        NewDayAction_ValueChanged(this, NewDayAction.Value);
    }

    private void NewDayAction_ValueChanged(object sender, string e)
    {
        NewDayActionValue = Enum.Parse<NewDayActionValue>(e);
    }

    void SetCondition(ModSetting<bool> s, bool isAll = false)
    {
        if (isAll)
        {
            s.Descriptor.SetEnableCondition(() => InternalImmediateHasAction);
        }
        else
        {
            s.Descriptor.SetEnableCondition(() => InternalImmediateHasAction && !ActionOnAllDays);
        }
    }

    ModSetting<bool> CreateNewDayCheck(string name, bool defaultOn = false)
        => new(defaultOn, ModSettingDescriptor.Create("  " + t.T("LV.TI.NewDay" + name)));

}
