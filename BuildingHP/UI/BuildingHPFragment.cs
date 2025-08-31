namespace BuildingHP.UI;

public class BuildingHPFragment(
    ITooltipRegistrar tooltipRegistrar,
    ILoc t,
    RenovationDialogController renovationDialogController,
    IDayNightCycle dayNightCycle,
    BuildingRenovationElementDependencies buildingRenovationElementDependencies,
    RenovationSpecService renovationSpecService,
    RenovationPriorityToggleGroupFactory renovationPriorityToggleGroupFactory
) : BaseEntityPanelFragment<BuildingHPComponent>
{

#nullable disable
    ProgressBar pgb;
    Label lblHp;
    Button btnRenovate;
    BuildingRenovationElement renoPanel;
    RenovationListElement renovationListPanel;
    BuildingRenovationEffectPanel effectPanel;
    AutoRepairPanel autoRepairPanel;
#nullable enable

    public VisualElement Panel => panel;
    BuildingRenovationComponent? reno;
    BuildingHPRepairComponent? repair;

    protected override void InitializePanel()
    {
        pgb = panel.AddProgressBar("HPBar").SetMarginBottom(10);
        lblHp = pgb.AddProgressLabel("HP: 0 / 0", "HPBarLabel");
        tooltipRegistrar.Register(pgb, GetTooltipContent);

        btnRenovate = panel
            .AddStretchedEntityFragmentButton(t.T("LV.BHP.Renovate"), onClick: OnRenovateRequested, color: EntityFragmentButtonColor.Red)
            .SetMarginBottom();

        renoPanel = panel.AddChild<BuildingRenovationElement>(() => new(buildingRenovationElementDependencies))
            .SetMarginBottom();

        effectPanel = panel.AddChild<BuildingRenovationEffectPanel>(() => new(t, dayNightCycle))
            .SetMarginBottom();

        autoRepairPanel = panel.AddChild<AutoRepairPanel>(() => new(t, renovationPriorityToggleGroupFactory))
            .SetMarginBottom();

        renovationListPanel = panel.AddChild<RenovationListElement>(() => new(t, dayNightCycle, renovationSpecService));
    }

    public override void ShowFragment(BaseComponent entity)
    {
        base.ShowFragment(entity);
        if (!component) { return; }

        reno = entity.GetRenovationComponent();
        repair = entity.GetComponentFast<BuildingHPRepairComponent>();

        if (!reno || !repair)
        {
            ClearFragment();
            return;
        }

        renoPanel.SetComponent(reno);
        renovationListPanel.SetComponent(reno);
        effectPanel.SetComponent(reno);
        autoRepairPanel.SetComponent(repair);
        UpdateFragment();
    }

    public override void UpdateFragment()
    {
        if (!component) { return; }

        var perc = component.HPPercent;
        var invulnerable = component.Invulnerable;
        pgb.SetProgress(perc,
            label: lblHp, text: t.T(
                invulnerable ? "LV.BHP.HPInvul" : "LV.BHP.HP",
                component.HP, component.Durability));

        renoPanel.Update();
        effectPanel.Update();
        autoRepairPanel.Update();
        btnRenovate.SetDisplay(reno!.CanRenovate);
    }

    public override void ClearFragment()
    {
        base.ClearFragment();
        renoPanel.Unset();
        renovationListPanel.Unset();
        effectPanel.Unset();
        autoRepairPanel.Unset();
        reno = null;
        repair = null;
    }

    string GetTooltipContent()
    {
        if (!component) { return "N/A"; } // Should not happen

        var durabilityList = new StringBuilder();
        foreach (var desc in component.DurabilityDescriptions)
        {
            if (desc.Type == BuildingDurabilityModifierType.Invulnerability)
            {
                durabilityList.AppendLine(AppendTime(t.T("LV.BHP.HPTooltipInvul", t.T(desc.DescriptionKey)), desc.EndTime));
                continue;
            }

            var valueDisplay = desc.Type switch
            {
                BuildingDurabilityModifierType.Addition => desc.Value.ToString("+0;-0"),
                BuildingDurabilityModifierType.Multiplier => desc.Value.ToString("+0%;-0%"),
                _ => null,
            };
            if (valueDisplay is null) { continue; }

            durabilityList.AppendLine(AppendTime(t.T("LV.BHP.HPTooltipDurability", valueDisplay, t.T(desc.DescriptionKey)), desc.EndTime));
        }

        return t.T("LV.BHP.HPTooltip",
            component.HP,
            component.Durability,
            durabilityList.ToString());

        string AppendTime(string text, float? endTime)
        {
            if (endTime is null) { return text; }

            var remainingTime = endTime.Value - dayNightCycle.PartialDayNumber;
            return $"{text} (⏱️ {t.T("Time.DaysShort", remainingTime.ToString("0.00"))})";
        }
    }

    async void OnRenovateRequested()
    {
        if (!reno) { return; }

        await renovationDialogController.OpenDialogAsync(reno);
    }

}
