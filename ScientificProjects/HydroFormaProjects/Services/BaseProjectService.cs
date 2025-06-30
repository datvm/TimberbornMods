namespace HydroFormaProjects.Services;

public abstract class BaseProjectService(ScientificProjectService projects)
{

    bool canUseProject;

    protected abstract string ProjectId { get; }
    public bool CanUseProject => canUseProject || ReloadCanUseProject();

    bool ReloadCanUseProject() =>
        canUseProject = projects.IsUnlocked(ProjectId);

}
