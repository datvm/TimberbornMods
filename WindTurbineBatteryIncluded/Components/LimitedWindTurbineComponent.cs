namespace WindTurbineBatteryIncluded.Components;

[AddTemplateModule2(typeof(LimitedWindTurbineSpec))]
public class LimitedWindTurbineComponent(LimitedWindTurbineService service, EventBus eb) : BaseComponent,
    IInitializableEntity, IDeletableEntity, IAwakableComponent, IFinishedStateListener
{
    LimitedWindTurbineSpec spec = null!;
    LimitedWindTurbineYaw yaw = null!;
    MechanicalNode mechanicalNode = null!;
    MechanicalNodeSpec nodeSpec = null!;

    public LimitedWindTurbineSpec Spec => spec;
    public int NominalPower => nodeSpec.PowerOutput;
    public float YawError => yaw.YawError;

    public void Awake()
    {
        spec = GetComponent<LimitedWindTurbineSpec>();
        yaw = GetComponent<LimitedWindTurbineYaw>();
        mechanicalNode = GetComponent<MechanicalNode>();
        nodeSpec = GetComponent<MechanicalNodeSpec>();
    }

    public void InitializeEntity()
    {
        eb.Register(this);
        yaw.CaptureRest();
        UpdateWind();
    }

    public void DeleteEntity()
    {
        eb.Unregister(this);
    }

    public void OnEnterFinishedState()
    {
        yaw.CaptureRest();
        UpdateWind();
    }

    public void OnExitFinishedState()
    {
    }

    [OnEvent]
    public void OnWindChanged(WindChangedEvent _)
    {
        UpdateWind();
    }

    void UpdateWind()
    {
        if (!yaw.RestCaptured)
        {
            return;
        }

        mechanicalNode.SetOutputMultiplier(service.OutputMultiplier(yaw.YawError, spec));
    }
}
