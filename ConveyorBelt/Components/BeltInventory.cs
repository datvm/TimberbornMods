namespace ConveyorBelt.Components;

[AddTemplateModule2(typeof(BeltSystemSpec))]
public class BeltInventory(BeltInventoryService service)
    : BaseComponent, IAwakableComponent, IInitializableEntity, IFinishedStateListener, IPersistentEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BeltInventory));
    static readonly ListKey<string> ItemsKey = new("Items");
    static readonly PropertyKey<bool> WarnKey = new("Warn");

    BlockObject block = null!;
    SimBelt? lane;
    StatusToggle? stuckStatus;

    public IReadOnlyList<BeltGood> Items => Lane.Items;
    public bool IsStuck => Lane.IsStuck;
    public bool WarnWhenStuck { get; set; }

    SimBelt Lane => lane ??= FindLane();

    public void Awake() => block = GetComponent<BlockObject>();

    public void InitializeEntity()
    {
        stuckStatus = StatusToggle.CreateNormalStatusWithAlert(
            "LackOfResources",
            service.t.T("LV.CBlt.Stuck"),
            service.t.T("LV.CBlt.StuckShort"),
            1f);
        GetComponent<StatusSubject>().RegisterStatus(stuckStatus);
    }

    public void OnEnterFinishedState() => service.Register(this);

    public void OnExitFinishedState()
    {
        service.Unregister(this);
        Eject();
        stuckStatus?.Deactivate();
    }

    public void Eject()
    {
        if (Lane.Items.Count > 0)
        {
            service.Drop(block.Coordinates, Lane.Items);
        }

        Lane.Clear();
        ApplyStuck();
    }

    public void ApplyStuck()
    {
        if (stuckStatus is null)
        {
            return;
        }

        if (WarnWhenStuck && Lane.IsStuck)
        {
            stuckStatus.Activate();
        }
        else
        {
            stuckStatus.Deactivate();
        }
    }

    public void Save(IEntitySaver saver)
    {
        var s = saver.GetComponent(SaveKey);
        if (Lane.Items.Count > 0)
        {
            List<string> serialized = [];
            foreach (var item in Lane.Items)
            {
                serialized.Add(item.Serialize());
            }

            s.Set(ItemsKey, serialized);
        }

        if (WarnWhenStuck)
        {
            s.Set(WarnKey, true);
        }
    }

    public void Load(IEntityLoader loader)
    {
        if (!loader.TryGetComponent(SaveKey, out var s))
        {
            return;
        }

        if (s.Has(ItemsKey))
        {
            Lane.Items.Clear();
            foreach (var text in s.Get(ItemsKey))
            {
                if (BeltGood.TryDeserialize(text, out var good))
                {
                    Lane.Items.Add(good);
                }
            }
        }

        if (s.Has(WarnKey))
        {
            WarnWhenStuck = s.Get(WarnKey);
        }
    }

    SimBelt FindLane()
    {
        if (GetComponent<BeltCarrier>() is { } carrier)
        {
            return carrier.Sim;
        }

        if (GetComponent<BeltMerger>() is { } merger)
        {
            return merger.Sim.Belt;
        }

        if (GetComponent<BeltSplitter>() is { } splitter)
        {
            return splitter.Sim.Belt;
        }

        if (GetComponent<BeltLift>() is { } lift)
        {
            return lift.Sim.Belt;
        }

        return new();
    }
}
