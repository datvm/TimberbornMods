namespace ScientificProjects.Management;

partial class ScientificProjectService
{
    HashSet<string> unlockedProjects = [];
    public IEnumerable<string> UnlockedProjectIds => unlockedProjects;

    public string? CanUnlock(string id)
    {
        if (unlockedProjects.Contains(id))
        {
            return "LV.SP.UnlockErrUnlocked".T(t);
        }

        var proj = GetProjectSpec(id);

        if (proj.HasSteps)
        {
            return "LV.SP.UnlockErrUnlocked".T(t);
        }

        if (proj.RequiredId is not null && !IsUnlocked(proj.RequiredId))
        {
            var reqProj = GetProjectSpec(proj.RequiredId);

            return "LV.SP.UnlockErrRequirementLocked".T(t, reqProj.DisplayName);
        }

        // Unlockable projects cannot have scaling cost so at this step, only check for fixed cost
        if (sciences.SciencePoints < proj.ScienceCost)
        {
            return "LV.SP.UnlockErrCostLocked".T(t, sciences.SciencePoints, proj.ScienceCost);
        }

        if (proj.HasCustomUnlockCondition)
        {
            return CanCustomUnlock(GetProject(proj));
        }

        return null;
    }
    string? CanCustomUnlock(ScientificProjectInfo info)
    {
        var provider = registry.GetUnlockConditionProviderFor(info.Spec.Id);
        var condition = provider.CheckForUnlockCondition(info);

        return condition;
    }

    public bool IsUnlocked(string projectId)
    {
        if (unlockedProjects.Contains(projectId)) { return true; }

        var proj = GetProjectSpec(projectId);
        return proj.HasSteps &&
            (proj.RequiredId is null || IsUnlocked(proj.RequiredId));
    }
    public bool IsPreqUnlocked(ScientificProjectSpec p) => p.RequiredId is null || IsUnlocked(p.RequiredId);

    public string? TryToUnlock(string id, bool force = false)
    {
        if (!force)
        {
            var unlockBlocked = CanUnlock(id);
            if (unlockBlocked is not null)
            {
                return unlockBlocked;
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
