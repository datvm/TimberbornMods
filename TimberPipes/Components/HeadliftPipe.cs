namespace TimberPipes.Components;

[AddTemplateModule2(typeof(PipeHeadliftSpec))]
public class HeadliftPipe : BaseComponent, IAwakableComponent
{
#nullable disable
    PipeHeadliftSpec spec;
#nullable enable

    MechanicalBuilding? mech;
    PausableBuilding? pausable;

    public float RatedMaxHeadLift => spec.MaxHeadLift;
    public int? InjectRate => spec.InjectRate;
    public bool IsPaused => pausable is { Paused: true };

    public float WorkFactor
    {
        get
        {
            if (pausable is { Paused: true })
            {
                return 0f;
            }

            if (mech)
            {
                return mech!.ActiveAndPowered ? mech.Efficiency : 0f;
            }

            return 1f;
        }
    }

    public float EffectiveMaxHeadLift => RatedMaxHeadLift * WorkFactor;

    public void Awake()
    {
        spec = GetComponent<PipeHeadliftSpec>();
        mech = this.GetComponentOrNull<MechanicalBuilding>();
        pausable = this.GetComponentOrNull<PausableBuilding>();
    }
}
