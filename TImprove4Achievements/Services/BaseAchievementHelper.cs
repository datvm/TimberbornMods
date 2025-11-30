namespace TImprove4Achievements.Services;

public abstract class BaseAchievementHelper
{
    public abstract string Id { get; }
    public abstract string NameLoc { get; }
    public abstract string DescLoc { get; }
    public bool Secret { get; }

    public bool Enabled => Achievement.IsEnabled;

    public abstract Type AchievementType { get; }
    public Achievement Achievement { get; protected set; } = null!;    

    internal abstract void SetAchievement(Achievement achievement);
}

public abstract class BaseAchievementHelper<T> : BaseAchievementHelper where T : Achievement
{

    public override Type AchievementType => typeof(T);
    public new T Achievement { get; private set; } = null!;

    internal override void SetAchievement(Achievement achievement)
    {
        base.Achievement = achievement;
        Achievement = (T)achievement;
    }

}