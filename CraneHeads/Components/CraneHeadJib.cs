namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadJibSpec))]
public class CraneHeadJib(ILoc t) : BaseComponent, IAwakableComponent, IDeletableEntity, ICraneRangeModifier, IEntityDescriber
{
    CraneHeadJibSpec spec = null!;
    CraneHeadComponent head = null!;
    CraneComponent? appliedTo;

    public int RangeDelta => spec.ExtraRange;

#pragma warning disable CS0067 // ExtraRange is spec-constant; raise this if a head's delta becomes dynamic.
    public event EventHandler? OnRangeChanged;
#pragma warning restore CS0067

    public void Awake()
    {
        spec = GetComponent<CraneHeadJibSpec>();
        head = GetComponent<CraneHeadComponent>();
        head.CraneChanged += OnCraneChanged;
    }

    public void DeleteEntity()
    {
        head.CraneChanged -= OnCraneChanged;
        ClearModifier();
    }

    public IEnumerable<EntityDescription> DescribeEntity() 
        => [EntityDescription.CreateTextSection(t.T("LV.CrH.HeadRangeBonus", spec.ExtraRange), 30)];

    void OnCraneChanged(object sender, EventArgs e)
    {
        if (head.Crane is not { } target || !target)
        {
            ClearModifier();
            return;
        }

        if (appliedTo is not null)
        {
            throw new InvalidOperationException("This jib is already applied to a crane.");
        }

        target.AddRangeModifier(this);
        appliedTo = target;
    }

    void ClearModifier()
    {
        if (appliedTo is not null && appliedTo)
        {
            appliedTo.RemoveRangeModifier(this);
        }

        appliedTo = null;
    }
}
