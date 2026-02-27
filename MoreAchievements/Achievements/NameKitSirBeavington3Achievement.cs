namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class NameKitSirBeavington3Achievement(EventBus eb) : EbAchievementBase(eb)
{
    public static string AchId = "LV.MA.NameKitSirBeavington3";
    public const string RequiredName = "Sir Beavington the 3rd";

    public override string Id => AchId;

    [OnEvent]
    public void OnEntityNamed(EntityNameChangedEvent e)
    {
        var entity = e.Entity;
        if (entity.GetCharacterType() != CharacterType.ChildBeaver) { return; }
        if (entity.GetName(null!) != RequiredName) { return; }

        Unlock();
    }

}
