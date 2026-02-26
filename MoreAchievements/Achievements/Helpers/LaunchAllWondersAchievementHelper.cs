namespace MoreAchievements.Achievements.Helpers;

[MultiBind(typeof(BaseAchievementHelper))]
public class LaunchAllWondersAchievementHelper(DialogService diag, ILoc t)
    : BaseMessageBoxAchievementHelper<LaunchAllWondersAchievement>("LV.MA.LaunchAllWonders", 1, diag, t)
{
    protected override object[] GetParameters(int step) => throw new NotImplementedException();

    protected override string GetMessage(int step)
    {
        var a = Achievement;

        var launched = a.LaunchedTemplate.Count == 0
            ? t.TNone()
            : GetBuildingNames(a.LaunchedTemplate.Select(t => a.WonderTemplates[t]));
        
        var unlaunchedTemplates = a.WonderTemplates.Keys.Where(t => !a.LaunchedTemplate.Contains(t));
        var unlaunched = GetBuildingNames(unlaunchedTemplates.Select(t => a.WonderTemplates[t]));

        return t.T("LV.MA.LaunchAllWonders.Msg0Launched", launched, unlaunched);
    }

    string GetBuildingNames(IEnumerable<TemplateSpec> templates)
        => string.Join(", ", templates.Select(GetBuildingName));

    string GetBuildingName(TemplateSpec spec)
        => t.T(spec.GetSpec<LabeledEntitySpec>().DisplayNameLocKey);

}
