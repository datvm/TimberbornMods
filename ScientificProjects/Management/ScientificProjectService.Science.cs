namespace ScientificProjects.Management;

partial class ScientificProjectService
{
    HashSet<string> unlockedProjects = [];
    public IEnumerable<string> UnlockedProjectIds => unlockedProjects;

    public ScientificProjectUnlockStatus CanUnlock(string id)
    {
        if (unlockedProjects.Contains(id))
        {
            return ScientificProjectUnlockStatus.Unlocked;
        }

        var proj = GetProjectSpec(id);

        if (proj.HasSteps)
        {
            return ScientificProjectUnlockStatus.Unlocked;
        }

        if (proj.RequiredId is not null && !IsUnlocked(proj.RequiredId))
        {
            return ScientificProjectUnlockStatus.RequirementLocked;
        }

        // Unlockable projects cannot have scaling cost so at this step, only check for fixed cost
        return sciences.SciencePoints > proj.ScienceCost
            ? ScientificProjectUnlockStatus.CanUnlock
            : ScientificProjectUnlockStatus.CostLocked;
    }

    public bool IsUnlocked(string projectId)
    {
        if (unlockedProjects.Contains(projectId)) { return true; }

        var proj = GetProjectSpec(projectId);
        return proj.HasSteps &&
            (proj.RequiredId is null || IsUnlocked(proj.RequiredId));
    }
    public bool IsPreqUnlocked(ScientificProjectSpec p) => p.RequiredId is null || IsUnlocked(p.RequiredId);

    public ScientificProjectUnlockStatus? TryToUnlock(string id, bool force = false)
    {
        if (!force)
        {
            var canUnlock = CanUnlock(id);

            if (canUnlock != ScientificProjectUnlockStatus.CanUnlock)
            {
                return canUnlock;
            }

            sciences.SubtractPoints(GetProjectSpec(id).ScienceCost);
        }

        PerformUnlock(id);
        return null;
    }

    public int GetCost(ScientificProjectSpec project)
    {
        var info = GetProject(project);
        var cost = GetCost(info);
        return cost;
    }

    /// <summary>
    /// Make sure the info instance is up-to-date. If not, call <see cref="GetCost(ScientificProjectSpec)"/> instead.
    /// </summary>
    public int GetCost(ScientificProjectInfo info)
    {
        if (info.Spec.HasScalingCost)
        {
            var provider = registry.GetCostProviderFor(info.Spec.Id);            
            return provider.CalculateCost(info);
        }

        return info.Spec.ScienceCost;
    }

    void PerformUnlock(string id)
    {
        unlockedProjects.Add(id);

        eb.Post(new OnScientificProjectUnlockedEvent(GetProjectSpec(id)));
    }

}
