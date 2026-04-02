namespace MoreAchievements.Components;

[AddTemplateModule2(typeof(WonderSpec))]
public class WonderCompletionTracker(IDayNightCycle dayNightCycle) : BaseComponent, IAwakableComponent, IFinishedStateListener, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(WonderCompletionTracker));
    static readonly PropertyKey<int> BuiltDayKey = new("BuiltDay");

    public int BuiltDay { get; private set; }
    public bool LaunchedOnTheSameDay { get; private set; }

    public void Awake()
    {
        var w = GetComponent<Wonder>();
        w.WonderActivated += OnWonderActivated;
    }

    void OnWonderActivated(object sender, EventArgs e)
    {
        LaunchedOnTheSameDay = dayNightCycle.DayNumber == BuiltDay;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(BuiltDayKey))
        {
            BuiltDay = s.Get(BuiltDayKey);
        }
    }

    public void OnEnterFinishedState()
    {
        if (BuiltDay == 0)
        {
            BuiltDay = dayNightCycle.DayNumber;
        }
    }
    public void OnExitFinishedState() { }

    public void Save(IEntitySaver entitySaver)
    {
        if (BuiltDay == 0) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(BuiltDayKey, BuiltDay);
    }
}
