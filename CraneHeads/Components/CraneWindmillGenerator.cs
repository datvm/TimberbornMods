namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneComponent))]
public class CraneWindmillGenerator(
    WindService wind,
    CraneStructureService structures
) : TickableComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity
{
    MechanicalNode node = null!;
    CraneComponent crane = null!;
    CraneHeadWindmill? head;

    public int BasePowerOutput => node._mechanicalNodeSpec.PowerOutput;
    public int CurrentPowerOutput => node.Actuals.PowerOutput;
    public int SectionCount => crane.Tower.Sections.Count;
    public bool HasHead => head is not null;

    public void Awake()
    {
        node = GetComponent<MechanicalNode>();
        crane = GetComponent<CraneComponent>();
        node.SetOutputMultiplier(0f);
    }

    public void InitializeEntity() => UpdateOutput();

    public void DeleteEntity() => ClearHead();

    public void SetHead(CraneHeadWindmill? next)
    {
        if (head == next)
        {
            return;
        }

        if (head is not null && next is not null)
        {
            throw new InvalidOperationException("This crane already has a windmill.");
        }

        if (next is null)
        {
            ClearHead();
            return;
        }

        head = next;
        structures.OnCraneTowerChanged += OnTowerChanged;
        UpdateOutput();
    }

    public override void Tick() => UpdateOutput();

    void ClearHead()
    {
        if (head is null)
        {
            return;
        }

        structures.OnCraneTowerChanged -= OnTowerChanged;
        head = null;
        node.SetOutputMultiplier(0f);
    }

    void OnTowerChanged(object sender, CraneTower tower)
    {
        if (head is null || tower.Crane != crane)
        {
            return;
        }

        UpdateOutput();
    }

    void UpdateOutput()
    {
        if (head is null)
        {
            node.SetOutputMultiplier(0f);
            return;
        }

        var strength = wind.WindStrength;
        if (strength <= head.Spec.MinRequiredWindStrength)
        {
            node.SetOutputMultiplier(0f);
            return;
        }

        var heightFactor = 1f + head.Spec.BonusPerSection * crane.Tower.Sections.Count;
        node.SetOutputMultiplier(strength * heightFactor);
    }
}
