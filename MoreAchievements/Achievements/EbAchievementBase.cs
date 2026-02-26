namespace MoreAchievements.Achievements;

public abstract class EbAchievementBase(EventBus eb) : Achievement
{

    public override void EnableInternal()
    {
        eb.Register(this);
    }

    public override void DisableInternal()
    {
        eb.Unregister(this);
    }

}
