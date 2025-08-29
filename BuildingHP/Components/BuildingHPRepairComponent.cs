namespace BuildingHP.Components;

public class BuildingHPRepairComponent : TickableComponent, IPersistentEntity
{
    static readonly PropertyKey<bool> autoRepairKey = new("AutoRepair");
    static readonly PropertyKey<int> autoRepairThresholdKey = new("AutoRepairThreshold");
    static readonly PropertyKey<int> repairPriorityKey = new("AutoRepairPriority");

#nullable disable
    public BuildingRenovationComponent RenovationComponent { get; private set; }
#nullable enable

    public BuildingHPComponent BuildingHPComponent => RenovationComponent.BuildingHPComponent;
    public BuildingRepairService BuildingRepairService => BuildingHPComponent.BuildingHPService.BuildingRepairService;

    public bool AutoRepair { get; set; }
    public int AutoRepairThreshold { get; set; }
    public BuildingRepairInfo RepairInfo { get; private set; }
    public Priority AutoRepairPriority { get; set; }

    Action<BuildingRepairInfo>? pending;

    public bool NeedRepair
    {
        get
        {
            var missing = 1 - BuildingHPComponent.HPPercent;
            return missing >= RepairInfo.HPPercent;
        }
    }
    public bool CanRepair => RenovationComponent.CanRenovate && NeedRepair;

    public int PossibleRepairAmount
    {
        get
        {
            var missing = 1 - BuildingHPComponent.HPPercent;
            return Mathf.FloorToInt(missing / RepairInfo.HPPercent);
        }
    }

    public void Awake()
    {
        RenovationComponent = this.GetRenovationComponent();
    }

    public override void StartTickable()
    {
        var building = GetComponentFast<BuildingSpec>();
        RepairInfo = BuildingRepairService.CalculateRepairCost(building.BuildingCost);

        pending?.Invoke(RepairInfo);
        pending = null;
    }

    public override void Tick()
    {
        CheckForAutoRepair();
    }

    public void RequestRepair(int amount, Priority priority)
    {
        if (!CanRepair) { return; }
        BuildingRepairService.Repair(this, amount, priority);
    }

    public void RequesetMaxRepair(Priority priority)
    {
        var amount = PossibleRepairAmount;
        if (amount <= 0) { return; }

        RequestRepair(amount, priority);
    }

    public void RequestRepairInfo(Action<BuildingRepairInfo> callback)
    {
        if (RepairInfo == default)
        {
            pending = callback;
        }
        else
        {
            callback(RepairInfo);
        }
    }

    void CheckForAutoRepair()
    {
        if (!AutoRepair) { return; }

        var hp = BuildingHPComponent.HPPercentInt;
        if (hp >= AutoRepairThreshold) { return; }

        RequesetMaxRepair(AutoRepairPriority);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(BuildingHPComponent.SaveKey, out var s)) { return; }

        if (s.Has(autoRepairKey))
        {
            AutoRepair = s.Get(autoRepairKey);
        }
        if (s.Has(autoRepairThresholdKey))
        {
            AutoRepairThreshold = s.Get(autoRepairThresholdKey);
        }
        if (s.Has(repairPriorityKey))
        {
            AutoRepairPriority = (Priority)s.Get(repairPriorityKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(BuildingHPComponent.SaveKey);
        s.Set(autoRepairKey, AutoRepair);
        s.Set(autoRepairThresholdKey, AutoRepairThreshold);
        s.Set(repairPriorityKey, (int)AutoRepairPriority);
    }

}
