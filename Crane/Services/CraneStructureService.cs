namespace Crane.Services;

[BindSingleton]
public class CraneStructureService(
    EntitySelectionService entitySelectionService,
    IBlockService blockService,
    BlockObjectSpawningHelper spawningHelper,
    ConstructionFactory constructionFactory,
    ISpecService specService,
    DefaultEntityTracker<CraneComponent> cranes
) : ILoadableSingleton, IPostLoadableSingleton
{
    public const int TowerRange = 1;

    readonly Dictionary<string, BlockObjectSpec> sectionSpecsByCraneTemplate = [];
    readonly Dictionary<string, BlockObjectSpec> sectionSpecsByName = [];
    bool postLoaded;

    public event EventHandler<CraneTower>? OnCraneTowerChanged;

    public void Load()
    {
        foreach (var section in specService.GetSpecs<CraneSectionSpec>())
        {
            var template = section.GetSpec<TemplateSpec>();
            var bo = section.GetSpec<BlockObjectSpec>();
            if (template is null || bo is null)
            {
                continue;
            }

            sectionSpecsByName[template.TemplateName] = bo;
            sectionSpecsByCraneTemplate[template.TemplateName.Replace("CraneSection.", "Crane.")] = bo;
        }
    }

    public void SelectCrane(CraneComponent? crane)
    {
        if (!crane) { return; }
        entitySelectionService.SelectAndFocusOn(crane);
    }

    public void SelectCraneOfSection(CraneSectionComponent? section)
    {
        if (!section || section!.Crane is not { } c) { return; }
        SelectCrane(c);
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

        var crane = s.Crane ?? FindCraneOfSection(s);
        if (crane is not null)
        {
            RefreshCraneStructure(crane);
        }
    }

    public void RefreshCraneStructure(
        CraneComponent c,
        CraneSectionComponent? ignoring = null)
    {
        if (!postLoaded) { return; }

        var tower = c.Tower;
        var changed = DetachSections(tower);

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
            if (!IsFinishedSection(section, ignoring))
            {
                break;
            }

            section.Tower = tower;
            tower.Sections.Add(section);
        }

        while (true)
        {
            var section = blockService.GetFirstObjectWithComponentAt<CraneSectionComponent>(coords);
            if (!IsUnfinishedSection(section, ignoring))
            {
                break;
            }

            section.Tower = tower;
            tower.UnderConstructionSections.Add(section);
            coords.z += 1;
        }

        UpdateWorkingBounds(tower);
        OnCraneTowerChanged?.Invoke(this, tower);
    }

    public void NotifyRangeChanged(CraneTower tower)
    {
        if (!postLoaded)
        {
            return;
        }

        UpdateWorkingBounds(tower);
        OnCraneTowerChanged?.Invoke(this, tower);
    }

    /// <summary>
    /// True if a Crane Section can be placed on the current top, including on an unfinished section.
    /// Vanilla <see cref="BlockValidator"/> already accepts unfinished stackable as matter below.
    /// </summary>
    public bool CanBuildHigher(CraneComponent crane)
    {
        if (!crane || !crane.IsFinished)
        {
            return false;
        }

        return TryGetNextSectionPlacement(crane, out var spec, out var placement)
            && spawningHelper.IsPlacementValid(spec, placement);
    }

    public bool TryBuildHigher(CraneComponent crane)
    {
        if (!CanBuildHigher(crane) || !TryGetNextSectionPlacement(crane, out var spec, out var placement))
        {
            return false;
        }

        constructionFactory.CreateAsUnfinished(new EntitySetup.Builder(spec.Blueprint), placement);
        return true;
    }

    public bool HasCraneBelow(CraneSectionComponent section)
    {
        if (!section)
        {
            return false;
        }

        var bo = section.GetComponent<BlockObject>();
        if (!bo || !bo.Positioned)
        {
            return false;
        }

        foreach (var obj in blockService.GetObjectsAt(section.Coordinates.Below()))
        {
            if (obj == bo)
            {
                continue;
            }

            if (obj.GetComponent<CraneComponent>() is { } c && c)
            {
                return true;
            }

            if (obj.GetComponent<CraneSectionComponent>() is { } s && s)
            {
                return true;
            }
        }

        return false;
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

    bool TryGetNextSectionPlacement(CraneComponent crane, [NotNullWhen(true)] out BlockObjectSpec? spec, out Placement placement)
    {
        spec = null;
        placement = default;

        var craneTemplate = crane.GetTemplateName();
        if (!sectionSpecsByCraneTemplate.TryGetValue(craneTemplate, out spec)
            && !sectionSpecsByName.TryGetValue(craneTemplate.Replace("Crane.", "CraneSection."), out spec))
        {
            return false;
        }

        var bo = crane.GetComponent<BlockObject>();
        placement = new(crane.Tower.TopIncludingUnfinished.Above(), bo.Orientation, bo.FlipMode);
        return true;
    }

    static bool DetachSections(CraneTower tower)
    {
        var changed = tower.Sections.Count > 0 || tower.UnderConstructionSections.Count > 0;
        foreach (var s in tower.Sections)
        {
            s.Tower = null;
        }

        tower.Sections.Clear();
        foreach (var s in tower.UnderConstructionSections)
        {
            s.Tower = null;
        }

        tower.UnderConstructionSections.Clear();
        return changed;
    }

    static bool IsFinishedSection(CraneSectionComponent? section, CraneSectionComponent? ignoring)
        => section && section != ignoring && section!.IsFinished;

    static bool IsUnfinishedSection(CraneSectionComponent? section, CraneSectionComponent? ignoring)
        => section && section != ignoring && !section!.IsFinished;

    static void UpdateWorkingBounds(CraneTower tower)
    {
        var origin = tower.Bottom;
        var topZ = tower.Top.z;

        var range = tower.HorizontalRange;
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
