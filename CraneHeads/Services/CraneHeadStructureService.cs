namespace CraneHeads.Services;

[BindSingleton]
public class CraneHeadStructureService(
    IBlockService blockService,
    CraneStructureService structureService,
    DefaultEntityTracker<CraneComponent> cranes
) : ILoadableSingleton, IPostLoadableSingleton, IUnloadableSingleton
{
    bool postLoaded;

    public void Load() => structureService.OnCraneTowerChanged += OnTowerChanged;

    public void Unload() => structureService.OnCraneTowerChanged -= OnTowerChanged;

    public void PostLoad()
    {
        postLoaded = true;

        foreach (var c in cranes.Entities)
        {
            RefreshCraneHead(c);
        }
    }

    public bool HasHead(CraneComponent crane) => GetHead(crane);

    public bool HasHead(CraneTower tower) => HasHead(tower.Crane);

    public CraneHeadComponent? GetHead(CraneComponent crane)
    {
        if (!crane)
        {
            return null;
        }

        var head = crane.GetComponent<CraneTowerHead>()?.Head;
        return head ? head : null;
    }

    public CraneHeadComponent? GetHead(CraneTower tower) => GetHead(tower.Crane);

    public CraneComponent? FindCrane(CraneHeadComponent head)
    {
        foreach (var obj in blockService.GetObjectsAt(head.AttachmentCoordinates.Below()))
        {
            if (obj.GetComponent<CraneComponent>() is { } c && c)
            {
                return c;
            }

            if (obj.GetComponent<CraneSectionComponent>() is { } s && s)
            {
                return s.GetCrane();
            }
        }

        return null;
    }

    public bool HasCraneSectionBelow(CraneHeadComponent head)
    {
        if (!head)
        {
            return false;
        }

        var bo = head.GetComponent<BlockObject>();
        if (!bo || !bo.Positioned)
        {
            return false;
        }

        var craneSection = blockService.GetFirstObjectWithComponentAt<CraneSectionComponent>(head.AttachmentCoordinates.Below());
        return craneSection;
    }

    public void RefreshHead(CraneHeadComponent head, CraneHeadComponent? ignoring = null)
    {
        if (!postLoaded)
        {
            return;
        }

        var crane = head.GetCrane();
        if (crane is not null && crane)
        {
            RefreshCraneHead(crane, ignoring);
        }
    }

    public void RefreshCraneHead(CraneComponent c, CraneHeadComponent? ignoring = null)
    {
        if (!postLoaded)
        {
            return;
        }

        var towerHead = c.GetComponent<CraneTowerHead>();
        if (!towerHead)
        {
            return;
        }

        if (!c.IsFinished)
        {
            towerHead.ClearHead();
            return;
        }

        var coords = c.Tower.Top.Above();
        var found = blockService.GetFirstObjectWithComponentAt<CraneHeadComponent>(coords);
        if (!IsAttachedHead(found, ignoring) || found!.AttachmentCoordinates != coords)
        {
            towerHead.ClearHead();
            return;
        }

        if (towerHead.HasHead)
        {
            return;
        }

        towerHead.SetHead(found);
    }

    void OnTowerChanged(object sender, CraneTower tower) => RefreshCraneHead(tower.Crane);

    static bool IsAttachedHead(CraneHeadComponent? head, CraneHeadComponent? ignoring)
        => head
            && head != ignoring
            && head!.IsFinished;
}
