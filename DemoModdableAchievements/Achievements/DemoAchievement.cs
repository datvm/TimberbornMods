namespace DemoModdableAchievements.Achievements;

public class DemoAchievement(EventBus eb) : Achievement
{

    public override string Id { get; } = nameof(DemoAchievement);

    public override void EnableInternal()
    {
        eb.Register(this);
    }

    public override void DisableInternal()
    {
        eb.Unregister(this);
    }

    [OnEvent]
    public void OnEntityDeleted(EntityDeletedEvent e)
    {
        if (e.Entity.HasComponent<BuildingSpec>())
        {
            Unlock();
        }
    }

}
