
namespace ModdableTimberbornDemo.Features.EnterableBuff;

public class DemoEnterableBuffComponent : TogglableEnterableTickEffectComponent<NeedManager>, IPersistentEntity, IEntityEffectDescriber
{
    const float AutoTurnOffInDays = 1f;
    static readonly ContinuousEffect WetFurEffect = new("WetFur", .3f);

    static readonly ComponentKey SaveKey = new(nameof(DemoEnterableBuffComponent));
    static readonly PropertyKey<float> TurnOffKey = new("TurnOffAt");

#nullable disable
    IDayNightCycle dayNightCycle;
#nullable enable
    float? turnOffAt;

    public int Order { get; }

    public override void Awake()
    {
        base.Awake();

        Toggled += OnToggled;

        Container.EntererAdded += Container_EntererAdded;
    }

    private void Container_EntererAdded(object sender, EntererAddedEventArgs e)
    {
        ModdableTimberbornUtils.LogVerbose(() => $"{e.Enterer} entered {Container}, total: {Enterers.Count}");
    }

    public override void StartTickable()
    {
        base.StartTickable();

        if (turnOffAt is not null) // Re-activate if it was active when saved
        {
            Toggle(true);
        }
    }

    private void OnToggled(bool active)
    {
        if (active)
        {
            // Use compound here in case it is activated by loading
            turnOffAt ??= dayNightCycle.PartialDayNumber + AutoTurnOffInDays;
        }
        else
        {
            turnOffAt = null;
        }
    }

    [Inject]
    public void Inject(IDayNightCycle dayNightCycle)
    {
        this.dayNightCycle = dayNightCycle;
    }

    protected override NeedManager GetData(Enterer enterer) => enterer.GetComponentFast<NeedManager>();

    protected override void TickEffect()
    {
        if (turnOffAt <= dayNightCycle.PartialDayNumber)
        {
            turnOffAt = null;
            Toggle(false);
            return;
        }

        var time = dayNightCycle.FixedDeltaTimeInHours;
        foreach (var e in EnterersData)
        {
            e.ApplyEffect(WetFurEffect, time);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!Active || turnOffAt is null) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(TurnOffKey, turnOffAt.Value);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        turnOffAt = s.Get(TurnOffKey);
    }

    public EntityEffectDescription? Describe(ILoc t, IDayNightCycle dayNightCycle)
        => Active
            ? new("Demo Enterable buff", $"{WetFurEffect.PointsPerHour:+0%} Wet Fur/h to Beavers inside", turnOffAt - dayNightCycle.PartialDayNumber)
            : null;

}
