namespace MoreAchievements.Achievements;

[MultiBind(typeof(Achievement))]
public class LaunchAllWondersAchievement(
    TemplateService templateService,
    EventBus eb,
    DefaultEntityTracker<Wonder> wonderTracker
) : EbAchievementBase(eb)
{
    public static string AchId = "LV.MA.LaunchAllWonders";
    public override string Id => AchId;

    public bool Available => WonderTemplates.Count > 1;
    public FrozenDictionary<string, TemplateSpec> WonderTemplates { get; private set; } = null!;
    readonly HashSet<string> launchedTemplates = [];
    public IReadOnlyCollection<string> LaunchedTemplate => launchedTemplates;

    public override void EnableInternal()
    {
        WonderTemplates = templateService
            .GetAll<WonderSpec>()
            .Select(w => w.GetSpec<TemplateSpec>())
            .ToFrozenDictionary(t => t.TemplateName);

        if (!Available)
        {
            Disable();
            return;
        }

        base.EnableInternal();
        wonderTracker.OnEntityRegistered += OnNewWonder;
    }

    void OnNewWonder(Wonder w)
    {
        w.WonderActivated += OnWonderActivated;
    }

    void OnWonderActivated(object sender, EventArgs e)
    {
        var template = ((Wonder)sender).GetTemplateName();
        launchedTemplates.Add(template);

        if (launchedTemplates.Count == WonderTemplates.Count)
        {
            Unlock();
        }
    }

    public override void DisableInternal()
    {
        base.DisableInternal();
        wonderTracker.OnEntityRegistered -= OnNewWonder;
        foreach (var w in wonderTracker.Entities)
        {
            w.WonderActivated -= OnWonderActivated;
        }
    }

    [OnEvent]
    public void OnNewCycle(CycleStartedEvent e)
    {
        launchedTemplates.Clear();
    }

}
