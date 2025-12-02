namespace BrainPowerSPs.Services;

public class WindPowerSPService(EntityRegistry entities) : IScientificProjectUnlockListener
{
    public FrozenSet<string> UnlockListenerIds { get; } = [PowerProjectsUtils.WindmillHeightUpId];
    public FrozenSet<string> ListenerIds { get; } = [PowerProjectsUtils.WindmillHeightUpId];

    public float BoostPerLevel { get; private set; } = 0;

    void SetActiveProject(ScientificProjectSpec spec)
    {
        BoostPerLevel = spec.Parameters[0];

        foreach (var e in entities.Entities)
        {
            var comp = e.GetComponent<WindPowerSPComponent>();
            if (comp)
            {
                comp.CalculateBoost();
            }
        }
    }

    public void OnListenerLoaded(IReadOnlyList<ScientificProjectInfo> activeProjects)
    {
        if (activeProjects.Count > 0)
        {
            SetActiveProject(activeProjects[0].Spec);
        }
    }

    public void OnProjectUnlocked(ScientificProjectSpec project, IReadOnlyList<ScientificProjectInfo> activeProjects) 
        => SetActiveProject(project);
}
