namespace DemoModdableAchievements.Achievements;

public class SecretAchievement(EventBus eb) : Achievement
{

    public override string Id { get; } = nameof(SecretAchievement);

    public override void EnableInternal()
    {
        eb.Register(this);
    }

    public override void DisableInternal()
    {
        eb.Unregister(this);
    }

    [OnEvent]
    public void OnDevModeToggled(DevModeToggledEvent e)
    {
        if (e.Enabled)
        {
            Unlock();
        }
    }

}
