namespace ScientificProjects.Services;

public class ScientificProjectUnlockService(
    ScientificProjectUnlockRegistry unlocks,
    ScientificProjectRegistry registry,
    FactionService factionService,
    ScienceService sciences,
    ILoc t,
    EventBus eb
) : ILoadableSingleton
{
    public event Action<ScientificProjectSpec>? OnProjectUnlocked;

    public IEnumerable<string> UnlockedProjectIds => unlocks.UnlockedProjectIds;

    public void Load()
    {
        FixUnlockedData();
        UnlockInitialProjects();
    }

    void UnlockInitialProjects()
    {
        foreach (var proj in registry.AllProjects)
        {
            if (proj.NeedUnlock) { continue; }

            if (proj.RequiredId is null || unlocks.Contains(proj.RequiredId))
            {
                unlocks.Unlock(proj.Id);
            }
        }
    }

    void FixUnlockedData()
    {
        foreach (var id in unlocks.UnlockedProjectIds.ToArray())
        {
            if (!registry.ProjectsById.ContainsKey(id))
            {
                Debug.LogWarning($"Unlocked Project {id} not found.");
                unlocks.Remove(id);
            }
        }
    }

    public string? CanUnlock(ScientificProjectSpec proj)
    {
        if (unlocks.Contains(proj.Id))
        {
            return "LV.SP.UnlockErrUnlocked".T(t);
        }

        if (!proj.NeedUnlock)
        {
            return "LV.SP.UnlockErrUnlocked".T(t);
        }

        if (!proj.IsAvailableTo(factionService.Current.Id))
        {
            return "LV.SP.WrongFaction".T(t);
        }

        if (proj.RequiredId is not null && !IsUnlocked(proj.RequiredId))
        {
            var reqProj = registry.GetProject(proj.RequiredId);

            return "LV.SP.UnlockErrRequirementLocked".T(t, reqProj.DisplayName);
        }

        // Unlockable projects cannot have scaling cost so at this step, only check for fixed cost
        if (sciences.SciencePoints < proj.ScienceCost)
        {
            return "LV.SP.UnlockErrCostLocked".T(t, sciences.SciencePoints, proj.ScienceCost);
        }

        if (proj.HasCustomUnlockCondition)
        {
            return CanCustomUnlock(proj);
        }

        return null;
    }
    string? CanCustomUnlock(ScientificProjectSpec project)
    {
        var provider = registry.GetUnlockConditionProviderFor(project.Id);
        var condition = provider.CheckForUnlockCondition(project);

        return condition;
    }

    public bool IsUnlocked(string id) => unlocks.Contains(id);
    public bool IsPreqUnlocked(ScientificProjectSpec p) => p.RequiredId is null || IsUnlocked(p.RequiredId);

    public string? TryToUnlock(ScientificProjectSpec project, bool force = false)
    {
        if (!force)
        {
            var unlockBlocked = CanUnlock(project);
            if (unlockBlocked is not null)
            {
                return unlockBlocked;
            }

            sciences.SubtractPoints(project.ScienceCost);
        }

        PerformUnlock(project);
        return null;
    }

    void PerformUnlock(ScientificProjectSpec project)
    {
        unlocks.Unlock(project.Id);
        OnProjectUnlocked?.Invoke(project);
        eb.Post(new OnScientificProjectUnlockedEvent(project));

        // Unlock any projects that can be auto-unlocked now
        var autoUnlocks = registry.ProjectsUnlockedById[project.Id];
        foreach (var p in autoUnlocks)
        {
            if (!p.NeedUnlock)
            {
                PerformUnlock(p);
            }
        }
    }

}
