namespace Ziporter.Components;

public enum ZiporterWarningState
{
    None,
    LosingConnection,
    ChargeLow,
    StabilizerDraining,
}

public class ZiporterWarning : TickableComponent
{

#nullable disable
    ZiporterWarningState currState;

    StatusToggle stabilizerDrainingStatus;
    StatusToggle chargeLowStatus;
    StatusToggle losingConnectionStatus;

    ZiporterStabilizer stabilizer;
    ZiporterBattery battery;

    ILoc t;
#nullable enable

    [Inject]
    public void Inject(ILoc t)
    {
        this.t = t;
    }

    public void Awake()
    {
        stabilizer = GetComponentFast<ZiporterStabilizer>();
        battery = GetComponentFast<ZiporterBattery>();
    }

    public override void StartTickable()
    {
        var subject = GetComponentFast<StatusSubject>();

        losingConnectionStatus = StatusToggle.CreatePriorityStatusWithAlertAndFloatingIcon(
            "ziporter-charge-draining",
            "LV.Ziporter.WarningChargeDraining".T(t, ZiporterConnection.DeactivateCapacity),
            "LV.Ziporter.WarningChargeDrainingShort".T(t));
        subject.RegisterStatus(losingConnectionStatus);

        stabilizerDrainingStatus = StatusToggle.CreatePriorityStatusWithAlertAndFloatingIcon(
            "stabilizer-draining",
            "LV.Ziporter.WarningDraining".T(t),
            "LV.Ziporter.WarningDrainingShort".T(t));
        subject.RegisterStatus(stabilizerDrainingStatus);

        chargeLowStatus = StatusToggle.CreatePriorityStatusWithAlertAndFloatingIcon(
            "stabilizer-charge-low",
            "LV.Ziporter.WarningChargeDrainingLow".T(t),
            "LV.Ziporter.WarningChargeDrainingLowShort".T(t));
        subject.RegisterStatus(chargeLowStatus);
    }

    public override void Tick()
    {
        ZiporterWarningState state = ZiporterWarningState.None;

        if (stabilizer.StabilizerNeeded)
        {
            state = ZiporterWarningState.StabilizerDraining;
        }
        else if (!battery.IsCharging)
        {
            var charge = battery.Charge;
            if (charge < ZiporterConnection.DeactivateCapacity)
            {
                state = ZiporterWarningState.ChargeLow;
            }
            else if (charge < ZiporterConnection.ActivateCapacity)
            {
                state = ZiporterWarningState.LosingConnection;
            }
        }

        if (state != currState) { SwitchToState(state); }
    }

    void SwitchToState(ZiporterWarningState state)
    {
        GetWarning(currState)?.Deactivate();
        GetWarning(state)?.Activate();
        currState = state;
    }

    StatusToggle? GetWarning(ZiporterWarningState state) => state switch
    {
        ZiporterWarningState.LosingConnection => losingConnectionStatus,
        ZiporterWarningState.ChargeLow => chargeLowStatus,
        ZiporterWarningState.StabilizerDraining => stabilizerDrainingStatus,
        _ => null,
    };
}
