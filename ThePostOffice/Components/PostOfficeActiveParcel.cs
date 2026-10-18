namespace ThePostOffice.Components;

[AddTemplateModule2(typeof(PostOfficeComponent))]
public class PostOfficeActiveParcel(
    PostOfficeGlobalInventoryService globalInventory,
    EventBus events
) : BaseComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity, IPostLoadableEntity, IPersistentEntity, IFinishedStateListener
{
    static readonly ComponentKey SaveKey = new(nameof(PostOfficeActiveParcel));
    static readonly PropertyKey<float> WorkedHoursKey = new("WorkedHours");
    static readonly PropertyKey<bool> SendHeldKey = new("SendHeld");
    static readonly PropertyKey<bool> PausedKey = new("Paused");

    PostOfficeSpec spec = null!;
    PostOfficeInventory building = null!;
    PostOfficeQueue queue = null!;
    bool sendHeld;
    bool heldOnSave;
    bool playerPaused;

    public float WorkedHours { get; private set; }
    public bool IsBusy { get; private set; }
    public bool SendHeld => sendHeld;
    public bool PlayerPaused => playerPaused;
    public bool Paused => playerPaused || queue.AutoSendSurplus();

    public float JobHours => queue.IsSend ? spec.SendHours : spec.RetrieveHours;

    public bool IsRunnable
    {
        get
        {
            if (!Ready() || playerPaused || queue.Front is null || WorkedHours >= JobHours)
            {
                return false;
            }

            if (queue.IsSend)
            {
                if (queue.AutoSendSurplus())
                {
                    return false;
                }

                return sendHeld && queue.FrontIsInStock();
            }

            return queue.FrontFits() && queue.GlobalHasFront();
        }
    }

    public void Awake()
    {
        spec = GetComponent<PostOfficeSpec>();
        building = GetComponent<PostOfficeInventory>();
        queue = GetComponent<PostOfficeQueue>();
    }

    public void InitializeEntity()
    {
        events.Register(this);
    }

    public void DeleteEntity()
    {
        events.Unregister(this);
    }

    public void OnEnterFinishedState()
    {
    }

    public void OnExitFinishedState()
    {
        ReleaseFront(false);
    }

    public void PostLoadEntity()
    {
        var drop = heldOnSave && queue.HasParcels && queue.IsSend && !queue.FrontIsInStock();
        heldOnSave = false;
        if (drop)
        {
            WorkedHours = 0f;
            queue.Cancel(0);
            return;
        }

        LookAgain();
    }

    public void LookAgain()
    {
        if (IsBusy || !Ready())
        {
            return;
        }

        IsBusy = true;
        try
        {
            while (TryAdvance())
            {
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Progress(float hours)
    {
        if (!IsRunnable || hours <= 0f)
        {
            return;
        }

        WorkedHours += hours;
        if (WorkedHours >= JobHours)
        {
            LookAgain();
        }
    }

    public void ReleaseFront(bool resetProgress)
    {
        if (sendHeld && queue.Front is PostOfficeParcel parcel)
        {
            globalInventory.Unreserve(parcel.Goods);
            foreach (var good in parcel.Goods)
            {
                building.Inventory.UnreserveStock(good);
            }

            sendHeld = false;
        }

        if (resetProgress)
        {
            WorkedHours = 0f;
            playerPaused = false;
        }
    }

    public void Pause()
    {
        playerPaused = true;
    }

    public void Resume()
    {
        if (!playerPaused)
        {
            return;
        }

        playerPaused = false;
        LookAgain();
    }

    public void Cancel()
    {
        queue.Cancel(0);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (WorkedHours == 0f && !sendHeld && !playerPaused)
        {
            return;
        }

        var saver = entitySaver.GetComponent(SaveKey);
        saver.Set(WorkedHoursKey, WorkedHours);
        saver.Set(SendHeldKey, sendHeld);
        saver.Set(PausedKey, playerPaused);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var loader))
        {
            return;
        }

        WorkedHours = loader.Has(WorkedHoursKey) ? loader.Get(WorkedHoursKey) : 0f;
        heldOnSave = loader.Has(SendHeldKey) && loader.Get(SendHeldKey);
        playerPaused = loader.Has(PausedKey) && loader.Get(PausedKey);
        sendHeld = false;
    }

    bool TryAdvance()
    {
        if (queue.Front is not PostOfficeParcel parcel)
        {
            ReleaseFront(true);
            return false;
        }

        if (queue.IsSend)
        {
            if (!queue.FrontIsInStock())
            {
                if (sendHeld)
                {
                    ReleaseFront(false);
                }

                return false;
            }

            if (!sendHeld)
            {
                if (!queue.FrontIsUnreserved() || !globalInventory.TryReserve(parcel.Goods))
                {
                    return false;
                }

                foreach (var good in parcel.Goods)
                {
                    building.Inventory.ReserveStock(good);
                }

                sendHeld = true;
            }

            if (playerPaused || queue.AutoSendSurplus())
            {
                return false;
            }

            if (WorkedHours < JobHours)
            {
                return false;
            }

            FinishSend(parcel);
            return true;
        }

        if (playerPaused)
        {
            return false;
        }

        if (sendHeld)
        {
            ReleaseFront(false);
        }

        if (!queue.FrontFits() || !queue.GlobalHasFront())
        {
            return false;
        }

        if (WorkedHours < JobHours)
        {
            return false;
        }

        FinishRetrieve(parcel);
        return true;
    }

    void FinishSend(PostOfficeParcel parcel)
    {
        foreach (var good in parcel.Goods)
        {
            building.Inventory.UnreserveStock(good);
            building.Inventory.TakeExisting(good);
        }

        globalInventory.AddReserved(parcel.Goods);
        sendHeld = false;
        WorkedHours = 0f;
        queue.RemoveFront();
    }

    void FinishRetrieve(PostOfficeParcel parcel)
    {
        globalInventory.Remove(parcel.Goods);
        foreach (var good in parcel.Goods)
        {
            building.Inventory.GiveExistingIgnoringCapacity(good);
        }

        WorkedHours = 0f;
        queue.RemoveFront();
    }

    bool Ready() => building.Inventory && building.Inventory.Enabled;
}
