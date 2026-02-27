namespace MoreAchievements.Services;

[BindSingleton]
public class AchievementWonderService(
    TemplateService templateService,
    DefaultEntityTracker<Wonder> wonderTracker
) : ILoadableSingleton
{
    public FrozenDictionary<string, TemplateSpec> WonderTemplates { get; private set; } = null!;
    public int WonderCount => WonderTemplates.Count;

    public IReadOnlyCollection<Wonder> ActiveWonders => wonderTracker.Entities;

    public event Action<Wonder>? OnWonderLaunched;

    public void Load()
    {
        WonderTemplates = templateService
            .GetAll<WonderSpec>()
            .Select(w => w.GetSpec<TemplateSpec>())
            .ToFrozenDictionary(t => t.TemplateName);

        wonderTracker.OnEntityRegistered += OnNewWonder;
        wonderTracker.OnEntityUnregistered += OnWonderUnregistered;
    }

    void OnWonderUnregistered(Wonder obj)
    {
        obj.WonderActivated -= WonderLaunchedHandler;
    }

    void OnNewWonder(Wonder w)
    {
        w.WonderActivated += WonderLaunchedHandler;
    }

    void WonderLaunchedHandler(object sender, EventArgs e)
    {
        OnWonderLaunched?.Invoke((Wonder)sender);
    }
}
