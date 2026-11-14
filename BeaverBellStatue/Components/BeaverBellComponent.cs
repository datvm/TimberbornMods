namespace BeaverBellStatue.Components;

public class BeaverBellComponent(EventBus eb)
    : BaseComponent, IAwakableComponent, IFinishedStateListener, IDeletableEntity, IUpdatableComponent
{
    const string ArmName = "Arm";

    // The bell clip holds its rest pose, then starts swinging as the hammer lands.
    const float HitTime = 6f / 30f;
    const float ReturnTime = 8f / 30f;

    const float ArmStrike = -70f;

    Transform arm = null!;
    Quaternion armRest;
    float elapsed;
    bool listening;

    public void Awake()
    {
        arm = GameObject.FindChildTransform(ArmName);
        armRest = arm.localRotation;
        DisableComponent();
    }

    public void OnEnterFinishedState()
    {
        eb.Register(this);
        listening = true;
    }

    public void OnExitFinishedState()
    {
        Unsubscribe();
        ResetPose();
    }

    public void DeleteEntity()
    {
        Unsubscribe();
    }

    public void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= HitTime + ReturnTime)
        {
            ResetPose();
            return;
        }

        float weight = elapsed <= HitTime
            ? Smooth(elapsed / HitTime)
            : 1f - Smooth((elapsed - HitTime) / ReturnTime);
        Apply(weight);
    }

    [OnEvent]
    public void OnWorkingHoursTransitioned(WorkingHoursTransitionedEvent _)
    {
        elapsed = 0f;
        Apply(0f);
        EnableComponent();
    }

    void Apply(float weight)
    {
        arm.localRotation = armRest * Quaternion.Euler(ArmStrike * weight, 0f, 0f);
    }

    void ResetPose()
    {
        Apply(0f);
        DisableComponent();
    }

    void Unsubscribe()
    {
        if (!listening)
        {
            return;
        }

        eb.Unregister(this);
        listening = false;
    }

    static float Smooth(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - (2f * t));
    }
}
