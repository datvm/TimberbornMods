namespace TimberPipes.Components;

[AddTemplateModule2(typeof(FlowLimitPipe))]
public class ThrottleGearAnimator : BaseComponent, IAwakableComponent, IUpdatableComponent, IFinishedStateListener
{
    const float DegreesPerCubicMeter = 360f;

#nullable disable
    IOutflowCounter counter;
    Transform gear;
#nullable enable

    public void Awake()
    {
        counter = GetComponent<IOutflowCounter>();
        TryBindGear();
        if (counter is null)
        {
            DisableComponent();
        }
    }

    public void OnEnterFinishedState()
    {
        TryBindGear();
        if (counter is null || gear is null)
        {
            DisableComponent();
            return;
        }

        EnableComponent();
    }

    public void OnExitFinishedState() => DisableComponent();

    void TryBindGear() => gear ??= NamedChild(Transform, "Gear");

    public void Update()
    {
        if (counter is null || gear is null)
        {
            return;
        }

        var volume = counter.TickVolume;
        if (volume <= 0f)
        {
            return;
        }

        gear.Rotate(0f, 0f, DegreesPerCubicMeter * volume * Time.deltaTime, Space.Self);
    }

    static Transform? NamedChild(Transform root, string name)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                return child;
            }
        }

        return null;
    }
}
