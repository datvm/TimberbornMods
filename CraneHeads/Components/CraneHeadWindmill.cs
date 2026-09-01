namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadWindmillSpec))]
public class CraneHeadWindmill(ILoc t) : BaseComponent, IAwakableComponent, IDeletableEntity, IEntityDescriber
{
    CraneHeadWindmillSpec spec = null!;
    CraneHeadComponent head = null!;
    CraneWindmillGenerator? appliedTo;

    public CraneHeadWindmillSpec Spec => spec;
    public CraneHeadComponent Head => head;
    public CraneWindmillGenerator? Generator => appliedTo;

    public void Awake()
    {
        spec = GetComponent<CraneHeadWindmillSpec>();
        head = GetComponent<CraneHeadComponent>();
        head.CraneChanged += OnCraneChanged;
    }

    public void DeleteEntity()
    {
        head.CraneChanged -= OnCraneChanged;
        ClearGenerator();
    }

    public IEnumerable<EntityDescription> DescribeEntity()
        => [EntityDescription.CreateTextSection(
            t.T("LV.CrH.WindmillHeightBonus", Mathf.RoundToInt(spec.BonusPerSection * 100f)),
            30)];

    void OnCraneChanged(object sender, EventArgs e)
    {
        if (head.Crane is not { } target || !target)
        {
            ClearGenerator();
            return;
        }

        if (appliedTo is not null)
        {
            throw new InvalidOperationException("This windmill is already applied to a crane.");
        }

        var generator = target.GetComponent<CraneWindmillGenerator>();
        generator.SetHead(this);
        appliedTo = generator;
    }

    void ClearGenerator()
    {
        if (appliedTo is not null && appliedTo)
        {
            appliedTo.SetHead(null);
        }

        appliedTo = null;
    }
}
