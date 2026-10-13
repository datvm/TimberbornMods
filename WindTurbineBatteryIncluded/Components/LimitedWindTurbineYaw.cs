namespace WindTurbineBatteryIncluded.Components;

[AddTemplateModule2(typeof(LimitedWindTurbineSpec))]
public class LimitedWindTurbineYaw(LimitedWindTurbineService service, EventBus eb) : BaseComponent,
    IInitializableEntity, IDeletableEntity, IAwakableComponent, IFinishedStateListener, IUpdatableComponent
{
    const string TowerName = "#LargeBaseMIsc";

    LimitedWindTurbineSpec spec = null!;
    Transform tower = null!;
    Quaternion restRotation;
    bool finished;

    public bool RestCaptured { get; private set; }
    public float YawError => RestCaptured ? Mathf.Abs(service.SignedYaw(restRotation)) : 0f;

    public void Awake()
    {
        spec = GetComponent<LimitedWindTurbineSpec>();
        DisableComponent();
    }

    public void InitializeEntity()
    {
        eb.Register(this);
        CaptureRest();
    }

    public void DeleteEntity()
    {
        eb.Unregister(this);
    }

    public void CaptureRest()
    {
        if (RestCaptured)
        {
            return;
        }

        Transform finishedModel = GameObject.FindChildTransform("#Finished");
        tower = finishedModel.gameObject.FindChildTransform(TowerName);
        restRotation = tower.rotation;
        RestCaptured = true;
    }

    public void OnEnterFinishedState()
    {
        finished = true;
        CaptureRest();
        SnapTower();
        BeginTurn();
    }

    public void OnExitFinishedState()
    {
        finished = false;
        DisableComponent();
    }

    public void Update()
    {
        if (tower == null || !RestCaptured)
        {
            DisableComponent();
            return;
        }

        float current = YawFromRest();
        float target = service.ClampedYaw(restRotation, spec.MaxDegree);
        float step = service.YawStep(Time.deltaTime);
        float remaining = Mathf.Abs(Mathf.DeltaAngle(current, target));
        if (step <= 0f || remaining <= step)
        {
            if (step > 0f)
            {
                tower.rotation = service.YawRotation(restRotation, target);
            }

            DisableComponent();
            return;
        }

        float next = Mathf.MoveTowards(current, target, step);
        tower.rotation = service.YawRotation(restRotation, next);
    }

    [OnEvent]
    public void OnWindChanged(WindChangedEvent _)
    {
        BeginTurn();
    }

    void BeginTurn()
    {
        if (!finished || tower == null || !RestCaptured)
        {
            return;
        }

        float current = YawFromRest();
        float target = service.ClampedYaw(restRotation, spec.MaxDegree);
        if (service.Wind.WindStrength <= 0f || Mathf.Abs(Mathf.DeltaAngle(current, target)) <= 0.001f)
        {
            DisableComponent();
            return;
        }

        EnableComponent();
    }

    void SnapTower()
    {
        if (tower == null || !RestCaptured)
        {
            return;
        }

        tower.rotation = service.YawRotation(restRotation, service.ClampedYaw(restRotation, spec.MaxDegree));
    }

    float YawFromRest()
    {
        Vector3 forward = Quaternion.Inverse(restRotation) * (tower.rotation * Vector3.forward);
        return Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
    }
}
