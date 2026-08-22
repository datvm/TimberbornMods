namespace CraneHeads.Components;

[AddTemplateModule2(typeof(CraneHeadTrebuchetSpec))]
public class CraneHeadTrebuchet(
    ILoc t,
    RecoveredGoodStackSpawner spawner,
    ITerrainService terrain
) : BaseComponent, IAwakableComponent, IDeletableEntity, ICraneRubbleProcessor, IEntityDescriber
{
    CraneHeadTrebuchetSpec spec = null!;
    CraneHeadComponent head = null!;
    CraneComponent? appliedTo;

    public void Awake()
    {
        spec = GetComponent<CraneHeadTrebuchetSpec>();
        head = GetComponent<CraneHeadComponent>();
        head.CraneChanged += OnCraneChanged;
    }

    public void DeleteEntity()
    {
        head.CraneChanged -= OnCraneChanged;
        ClearProcessor();
    }

    public IEnumerable<EntityDescription> DescribeEntity()
        => [EntityDescription.CreateTextSection(t.T("LV.CrH.TrebuchetLaunch", spec.LaunchDistance), 30)];

    public bool TryProcessRubble(CraneComponent crane, RecoveredGoodStack stack, int items)
    {
        if (!stack || !stack.Inventory)
        {
            return false;
        }

        List<GoodAmount> launched = [];
        foreach (var good in stack.Inventory.UnreservedStock().ToArray())
        {
            var available = stack.Inventory.UnreservedAmountInStock(good.GoodId);
            var amount = Math.Min(Math.Min(good.Amount, items), available);
            if (amount <= 0)
            {
                continue;
            }

            var moved = new GoodAmount(good.GoodId, amount);
            stack.Inventory.TakeExisting(moved);
            launched.Add(moved);
            items -= amount;
            if (items <= 0)
            {
                break;
            }
        }

        if (launched.Count > 0 && TryGetLanding(crane) is { } dest)
        {
            spawner.AddAwaitingGoods(dest, launched);
        }

        return true;
    }

    void OnCraneChanged(object sender, EventArgs e)
    {
        if (head.Crane is not { } target || !target)
        {
            ClearProcessor();
            return;
        }

        if (appliedTo is not null)
        {
            throw new InvalidOperationException("This trebuchet is already applied to a crane.");
        }

        target.AddRubbleProcessor(this);
        appliedTo = target;
    }

    void ClearProcessor()
    {
        if (appliedTo is not null && appliedTo)
        {
            appliedTo.RemoveRubbleProcessor(this);
        }

        appliedTo = null;
    }

    Vector3Int? TryGetLanding(CraneComponent crane)
    {
        var bo = crane.GetComponent<BlockObject>();
        var forward = bo.Orientation.Transform(Direction2D.Down.ToOffset());
        var dest = crane.Tower.Top.Above() + forward * spec.LaunchDistance;
        var size = terrain.Size;
        if (dest.x < 0 || dest.y < 0 || dest.x >= size.x || dest.y >= size.y)
        {
            return null;
        }

        return dest;
    }
}
