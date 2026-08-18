namespace Crane.Services;

[BindSingleton]
public class CraneStructureService(
    EntitySelectionService entitySelectionService,
    IBlockService blockService,
    DefaultEntityTracker<CraneComponent> cranes
) : IPostLoadableSingleton
{
    public const int TowerRange = 1;
    bool postLoaded;

    public event EventHandler<CraneTower>? OnCraneTowerChanged;

    public void SelectCraneOfSection(CraneSectionComponent? section)
    {
        if (!section || section!.Crane is not { } c) { return; }
        entitySelectionService.SelectAndFocusOn(c);
    }

    public void PostLoad()
    {
        postLoaded = true;

        foreach (var c in cranes.Entities)
        {
            RefreshCraneStructure(c);
        }
    }

    public void RefreshCraneSectionStructure(CraneSectionComponent s)
    {
        if (!postLoaded) { return; }

        if (s.IsFinished && s.Tower is null)
        {
            var crane = FindCraneOfSection(s);
            if (crane is not null)
            {
                RefreshCraneStructure(crane);
            }
        }
        else if (!s.IsFinished && s.Tower is not null)
        {
            RefreshCraneStructure(s.Tower.Crane);
        }
    }

    public void RefreshCraneStructure(CraneComponent c)
    {
        if (!postLoaded) { return; }

        var changed = false;

        var tower = c.Tower;

        if (tower.Sections.Count > 0)
        {
            foreach (var s in tower.Sections)
            {
                s.Tower = null;
            }

            tower.Sections.Clear();
            changed = true;
        }

        if (!c.IsFinished)
        {
            if (changed)
            {
                OnCraneTowerChanged?.Invoke(this, tower);
            }

            return;
        }

        var coords = c.Coordinates;
        while (true)
        {
            coords.z += 1;

            var section = blockService.GetFirstObjectWithComponentAt<CraneSectionComponent>(coords);
            if (!section || !section.IsFinished) { break; }

            section.Tower = tower;
            tower.Sections.Add(section);
        }

        UpdateWorkingBounds(tower);
        OnCraneTowerChanged?.Invoke(this, tower);
    }

    public CraneComponent? FindCraneOfSection(CraneSectionComponent s)
    {
        var coords = s.Coordinates;
        while (coords.z > 0)
        {
            coords.z -= 1;

            var hasSection = false;
            foreach (var obj in blockService.GetObjectsAt(coords))
            {
                if (obj.GetComponent<CraneComponent>() is { } c && c)
                {
                    return c;
                }

                if (obj.GetComponent<CraneSectionComponent>() is { } other && other)
                {
                    hasSection = true;
                    break;
                }
            }

            if (!hasSection) { return null; }
        }

        return null;
    }

    static void UpdateWorkingBounds(CraneTower tower)
    {
        var origin = tower.Bottom;
        var topZ = tower.Top.z;

        var range = TowerRange;
        // sizeZ is exclusive; +2 includes one layer above the mast (vanilla ConstructionSiteAccessible MinZ is baseZ - 1).
        tower.WorkingBounds = new(
            origin.x - range,
            origin.y - range,
            0,
            (range * 2) + 1,
            (range * 2) + 1,
            topZ + 2);

    }

}
