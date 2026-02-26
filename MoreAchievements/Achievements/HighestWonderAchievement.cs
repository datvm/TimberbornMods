namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class HighestWonderAchievement(
    DefaultEntityTracker<Wonder> wonderTracker,
    MapSize mapSize,
    TemplateService templateService
) : Achievement, ILoadableSingleton
{
    public const string AchId = "LV.MA.HighestWonder";

    public override string Id => AchId;

    public int RequiredHeight { get; private set; }

    public override void EnableInternal()
    {
        foreach (var w in wonderTracker.Entities)
        {
            w.WonderActivated += OnWonderActivated;
        }
        wonderTracker.OnEntityRegistered += TrackWonder;
    }

    public override void DisableInternal()
    {
        wonderTracker.OnEntityRegistered -= TrackWonder;
        foreach (var w in wonderTracker.Entities)
        {
            w.WonderActivated -= OnWonderActivated;
        }
    }

    void TrackWonder(Wonder w)
    {
        w.WonderActivated += OnWonderActivated;
    }

    void OnWonderActivated(object sender, EventArgs e)
    {
        var obj = ((Wonder)sender).GetComponent<BlockObject>();
        var z = obj.Coordinates.z;

        if (z >= RequiredHeight)
        {
            Unlock();
        }
    }

    public void Load()
    {
        var wonder = templateService.GetAll<WonderSpec>().FirstOrDefault()
            ?? throw new InvalidOperationException($"[{nameof(MoreAchievements)}] This faction does not have a wonder?");

        var wonderHeight = wonder.GetSpec<BlockObjectSpec>().Size.z;
        RequiredHeight = mapSize.TotalSize.z - wonderHeight;
    }

}
