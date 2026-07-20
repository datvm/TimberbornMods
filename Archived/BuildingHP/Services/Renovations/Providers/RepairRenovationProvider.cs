
namespace BuildingHP.Services.Renovations.Providers;

public class RepairRenovationProvider(DefaultRenovationProviderDependencies di) : DefaultRenovationProvider(di)
{
    static readonly PropertyKey<int> AmountKey = new("Amount");

#nullable disable
    GameSliderInt repairSlider;
    Label lblAmount;
#nullable enable

    public const string RenovationId = "Repair";
    public override string Id { get; } = RenovationId;

    public override string? CanRenovate(BuildingRenovationComponent building)
        => GetHPRepairComponent(building).NeedRepair
        ? null
        : t.T("LV.BHP.NoNeedRepair");

    protected override DefaultRenovationPanel CreateUIElement(BuildingRenovationComponent building)
    {
        var result = base.CreateUIElement(building);

        var repairPanel = result.Others.AddChild().SetMarginBottom();

        repairSlider = repairPanel.AddSliderInt(t.T("LV.BHP.RepairAmount"))
            .SetHorizontalSlider(new(1, 1, 1))
            .RegisterChange(OnRepairAmountChanged);

        lblAmount = repairPanel.AddGameLabel();

        return result;
    }

    protected override void UpdateUI(VisualElement element, BuildingRenovationComponent building)
    {
        base.UpdateUI(element, building);

        var repair = GetHPRepairComponent(building);
        var slider = repairSlider.Slider;
        var amount = slider.value = slider.highValue = repair.PossibleRepairAmount;
        OnRepairAmountChanged(amount);
    }

    void OnRepairAmountChanged(int amount)
    {
        var repair = GetHPRepairComponent(CurrentComponent!);

        var el = CurrentElement!;
        el.CostBox.SetMaterials([new GoodAmountSpec()
        {
            _goodId = repair.RepairInfo.Good,
            _amount = amount * repair.RepairInfo.Amount,
        }]);

        var hpComp = repair.BuildingHPComponent;
        var perc = repair.RepairInfo.HPPercent * amount;
        var addingHP = Mathf.CeilToInt(perc * hpComp.Durability);

        lblAmount.text = string.Format(t.T("LV.BHP.RepairHP"),
            perc,
            hpComp.HP, addingHP, hpComp.HP + addingHP);
    }

    protected override bool CreateRenovation(BuildingRenovationComponent building, DefaultRenovationPanel ve)
    {
        var repair = GetHPRepairComponent(building);
        repair.RequestRepair(repairSlider.Value, ve.Priority);

        return true;
    }

    static BuildingHPRepairComponent GetHPRepairComponent(BaseComponent comp) => comp.GetComponentFast<BuildingHPRepairComponent>();

    protected override void PerformSave(BuildingRenovationComponent comp, BuildingRenovation renovation, IObjectSaver s)
    {
        base.PerformSave(comp, renovation, s);

        var reno = (BuildingRepairRenovation)renovation;
        s.Set(AmountKey, reno.Amount);
    }

    protected override void PerformLoad(BuildingRenovationComponent comp, IObjectLoader s)
    {
        var repair = comp.GetComponentFast<BuildingHPRepairComponent>();
        repair.RequestRepairInfo(info =>
        {
            base.PerformLoad(comp, s);
        });
    }

    protected override BuildingRenovation CreateRenovationFromSave(BuildingRenovationComponent comp, IObjectLoader s)
    {
        var reno = new BuildingRepairRenovation(
            comp,
            comp.GetComponentFast<BuildingHPRepairComponent>().RepairInfo,
            s.Has(AmountKey) ? s.Get(AmountKey) : 1,
            RenovationSpec, di.BuildingRenovationDependencies);
        LoadRenovationProgress(reno, comp, s);

        return reno;
    }

}
