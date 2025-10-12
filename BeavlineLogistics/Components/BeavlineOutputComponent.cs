namespace BeavlineLogistics.Components;

public class BeavlineOutputComponent : BaseComponent, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BeavlineOutputComponent));
    static readonly PropertyKey<int> NextOutputIndexKey = new("NextOutputIndex");
    static readonly PropertyKey<float> ProgressKey = new("Progress");

#nullable disable
    BeavlineComponent beavline;
#nullable enable

    int nextOutputIndex;
    ITimeTrigger? timeTrigger;

    float pendingProgress;

    public float DaysPerItem { get; private set; } = 1f / 24f;

    public void Awake()
    {
        beavline = GetComponentFast<BeavlineComponent>();
    }

    public void Toggle(bool enabled)
    {
        if (enabled)
        {
            if (timeTrigger is not null && !timeTrigger.Finished) { return; }
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

    void FindAndMoveStuffOut()
    {
        var startingPoint = nextOutputIndex;

        var connected = beavline.ConnectedBuildings;
        if (connected.Count == 0)
        {
            nextOutputIndex = 0;
            return;
        }

        var outputGoods = beavline.GetOutputGoods();
        if (outputGoods.Count == 0) { return; }

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
        TimberUiUtils.LogDev($"{target} can receive {string.Join(", ", inputGoods)}");

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


}
