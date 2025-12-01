namespace TImprove4Achievements.Services.Helpers;

public abstract class BuildEveryStructureAchievementHelper<T>(
    DialogService diag,
    ILoc t,
    TemplateNameMapper templateNameMapper
) : BaseMessageBoxAchievementHelper<T>("LV.T4A.BuildAll", 2, diag, t) where T : BuildEveryStructureAchievement
{

    protected override object[] GetParameters(int step)
    {
        if (step == 0)
        {
            return [Achievement._structuresToBuild.Count];

        }
        else
        {
            var list = Achievement._structuresToBuild.Select(name =>
            {
                var template = templateNameMapper.GetTemplate(name);
                var label = template.GetSpec<LabeledEntitySpec>();
                return "- " + t.T(label.DisplayNameLocKey);
            }).OrderBy(q => q);

            return [Achievement._structuresToBuild.Count, string.Join("\n", list)];
        }
    }
}

public class BuildEveryStructureFolktailsAchievementHelper(DialogService diag, ILoc t, TemplateNameMapper templateNameMapper)
    : BuildEveryStructureAchievementHelper<BuildEveryStructureFolktailsAchievement>(diag, t, templateNameMapper)
{ }

public class BuildEveryStructureIronTeethAchievementHelper(DialogService diag, ILoc t, TemplateNameMapper templateNameMapper)
    : BuildEveryStructureAchievementHelper<BuildEveryStructureIronTeethAchievement>(diag, t, templateNameMapper)
{ }