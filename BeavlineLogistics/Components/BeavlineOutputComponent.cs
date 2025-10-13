
namespace BeavlineLogistics.Components;

// IActivableRenovationComponent is for the OutputSpeed renovation, NOT the output (which is handled by BeavlineComponent)
public class BeavlineOutputComponent : BaseComponent, IPersistentEntity, IActivableRenovationComponent
{
    static readonly ComponentKey SaveKey = new(nameof(BeavlineOutputComponent));
    static readonly PropertyKey<int> NextOutputIndexKey = new("NextOutputIndex");
    static readonly PropertyKey<float> ProgressKey = new("Progress");

#nullable disable
    internal BeavlineComponent beavline;
#nullable enable

    internal bool debugging;

    int nextOutputIndex;
    ITimeTrigger? timeTrigger;

    float pendingProgress;

    public const float DefaultDaysPerItem = 1f / 24f; // 1 item per hour
    float speedBonus;
    public float DaysPerItem { get; private set; } = DefaultDaysPerItem;

    public bool RenovationActive { get; private set; }
    public Action<BuildingRenovation>? ActiveHandler { get; set; }

    public void Awake()
    {
        beavline = GetComponentFast<BeavlineComponent>();
    }

    public void Start()
    {
        this.ActivateIfAvailable(BeavlineOutSpeedRenovationProvider.RenoId);
    }

    public void AddSpeedBonus(float multiplierAddition)
    {
        speedBonus += multiplierAddition;
        DaysPerItem = DefaultDaysPerItem / (1f + speedBonus);

        if (timeTrigger?.InProgress == true)
        {
            // Reset
            ScheduleNextStep();
        }
    }

    public void Toggle(bool enabled)
    {
        if (enabled)
        {
            if (timeTrigger is not null && timeTrigger.InProgress) { return; }

            TimberUiUtils.LogDev($"{this} is scheduling next output");
            ScheduleNextStep();
        }
        else
        {
            if (timeTrigger is null || timeTrigger.Finished) { return; }

            timeTrigger.Pause();
            timeTrigger = null;
        }
    }

    void ScheduleNextStep()
    {
        timeTrigger?.Pause();

        timeTrigger = beavline.BeavlineService.TimeTriggerFactory.Create(TriggerOutput, DaysPerItem);

        if (pendingProgress > 0)
        {
            timeTrigger.FastForwardProgress(pendingProgress);
            pendingProgress = 0;
        }
        timeTrigger.Resume();
    }

    void TriggerOutput()
    {
        FindAndMoveStuffOut();
        ScheduleNextStep();
    }

    internal void FindAndMoveStuffOut()
    {
        var connected = beavline.ConnectedBuildings;
        if (connected.Count == 0)
        {
            nextOutputIndex = 0;
            return;
        }

        var outputGoods = beavline.GetOutputGoods();
        if (outputGoods.Count == 0) { return; }

        if (debugging)
        {
            Debug.Log($"{this} is trying to send out goods {string.Join(", ", outputGoods)}");
        }

        if (nextOutputIndex >= beavline.ConnectedBuildings.Count) // Happens when a connected building is removed
        {
            nextOutputIndex = 0;
        }

        var startingPoint = nextOutputIndex;

        bool done;
        do
        {
            var building = connected[nextOutputIndex];
            done = TryGivingTo(building, outputGoods);

            nextOutputIndex++;
            if (nextOutputIndex >= connected.Count)
            {
                nextOutputIndex = 0;
            }
        } while (!done && nextOutputIndex != startingPoint);
    }

    bool TryGivingTo(BeavlineComponent target, HashSet<string> outputGoods)
    {
        var inputGoods = target.GetPossibleInputGoods(outputGoods);

        if (debugging)
        {
            Debug.Log($"- {target} can accept {string.Join(", ", inputGoods)}");
        }
        if (inputGoods.Count == 0) { return false; }

        var inv = target.InputInventory!;
        var srcInv = beavline.OutputInventory!;
        foreach (var g in inputGoods)
        {
            srcInv.Take(new(g, 1));
            inv.Give(new(g, 1));
        }

        return true;
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        if (s.Has(NextOutputIndexKey))
        {
            nextOutputIndex = s.Get(NextOutputIndexKey);
        }

        if (s.Has(ProgressKey))
        {
            pendingProgress = s.Get(ProgressKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (timeTrigger is null) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(NextOutputIndexKey, nextOutputIndex);
        s.Set(ProgressKey, timeTrigger.Progress);
    }

    public void Activate()
    {
        if (RenovationActive) { return; }
        RenovationActive = true;

        var reno = this.GetRenovationComponent();
        var spec = reno.RenovationService.GetSpec(BeavlineOutSpeedRenovationProvider.RenoId);
        AddSpeedBonus(spec.Parameters[0]);
    }
}
