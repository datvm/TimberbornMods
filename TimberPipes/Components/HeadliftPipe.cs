namespace TimberPipes.Components;

[AddTemplateModule2(typeof(PipeHeadliftSpec))]
public class HeadliftPipe(ILoc t) : BaseComponent, IAwakableComponent, IEntityDescriber
{
#nullable disable
    PipeHeadliftSpec spec;
#nullable enable

    MechanicalBuilding? mech;
    BlockableObject? blockable;

    public float RatedMaxHeadLift => spec.MaxHeadLift;
    public int? InjectRate => spec.InjectRate;
    public bool IsPaused => blockable is { IsUnblocked: false };

    public float WorkFactor
    {
        get
        {
            if (blockable is { IsUnblocked: false })
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
        blockable = this.GetComponentOrNull<BlockableObject>();
    }

    public IEnumerable<EntityDescription> DescribeEntity() => [
        EntityDescription.CreateTextSection(t.T("LV.TPi.ProvideHeadlift", RatedMaxHeadLift), 3),
    ];
}
