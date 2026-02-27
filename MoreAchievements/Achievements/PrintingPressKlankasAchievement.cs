namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class PrintingPressBotsAchievement(
    WorkplaceTracker workplaceTracker,
    TemplateNameMapper templateNameMapper
) : Achievement
{
    public static string AchId = "LV.MA.PrintingPressBots";
    public const string TemplateName = "PrintingPress.Folktails";

    public override string Id => AchId;

    public override void EnableInternal()
    {
        if (!templateNameMapper.TryGetTemplate(TemplateName, out _))
        {
            Disable();
            return;
        }

        workplaceTracker.OnWorkerAssigned += OnWorkerAssigned;
        foreach (var wp in workplaceTracker.Entities)
        {
            if (CheckWorkplace(wp.Workplace))
            {
                Unlock();
                return;
            }
        }
    }

    void OnWorkerAssigned(WorkplaceTrackerComponent wp, Worker wk)
    {
        if (CheckWorkplace(wp.Workplace))
        {
            Unlock();
        }
    }

    bool CheckWorkplace(Workplace wp)
    {
        if (wp.GetTemplateName() != TemplateName) { return false; }

        var workers = wp.AssignedWorkers;
        return workers.Count == wp.MaxWorkers && workers[0].GetCharacterType().IsBot();
    }

    public override void DisableInternal()
    {
        workplaceTracker.OnWorkerAssigned -= OnWorkerAssigned;
    }

}
