namespace BeaverChronicles.Services;

[BindSingleton]
public class ActiveChronicleEventService(
    ITimeTriggerFactory timerFac,
    ISingletonLoader loader,
    EventBus eb
) : ISaveableSingleton, ILoadableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(ActiveChronicleEventService));
    static readonly PropertyKey<float> ProgressKey = new("Progress");
    static readonly PropertyKey<string> PaymentKey = new("Payment");
    static readonly PropertyKey<string> DescriptionKey = new("Description");

    public float? SavedProgress { get; private set; }
    public ActiveEventPayment? SavedPayment { get; private set; }

    IChronicleEvent? ev;
    public IChronicleEvent? ActiveEvent => ev;

    ITimeTrigger? timeTrigger;
    public float RemainingDays => timeTrigger?.DaysLeft ?? 0;
    public bool HasTimeLimit => timeTrigger?.InProgress == true;

    public event EventHandler<IChronicleEvent?>? OnEventChanged;
    public event EventHandler<bool>? OnHasTimeLimitChanged;
    public event EventHandler<ActiveEventPayment?>? OnPaymentChanged;
    Action? onPaymentFinished;

    public ActiveEventPayment? Payment { get; private set; }
    public bool NeedsPayment => Payment?.Paid == false;

    public string? ActiveDescription { get; private set; }
    public event EventHandler<string?>? OnActiveDescriptionChanged;

    internal void SetActiveEvent(IChronicleEvent? ev)
    {
        if (this.ev == ev) { return; }
        this.ev = ev;
        OnEventChanged?.Invoke(this, ev);

        if (ev is null)
        {
            Clear();
        }
    }

    public void RegisterTimeLimit(float days, Action action)
    {
        if (ev is null)
        {
            throw new InvalidOperationException("No active event to register a time limit for.");
        }

        if (HasTimeLimit)
        {
            throw new InvalidOperationException("An active time limit is already registered. Clear it before registering a new one.");
        }

        timeTrigger = timerFac.CreateAndStart(() =>
        {
            action();
            OnHasTimeLimitChanged?.Invoke(this, false);
        }, days);

        if (HasTimeLimit)
        {
            OnHasTimeLimitChanged?.Invoke(this, true);
        }
    }

    public void RegisterSavedTimeLimit(float days, Action action)
    {
        if (SavedProgress is null)
        {
            throw new InvalidOperationException("No saved time limit to register.");
        }

        RegisterTimeLimit(days, action);
        FastForwardTimeLimit(SavedProgress.Value);
        SavedProgress = null;
    }

    public void FastForwardTimeLimit(float progress)
    {
        if (!HasTimeLimit)
        {
            throw new InvalidOperationException("No active time limit to fast forward.");
        }

        timeTrigger!.FastForwardProgress(progress);
    }

    public void ClearTimeLimit()
    {
        if (timeTrigger is null) { return; }

        if (timeTrigger.InProgress)
        {
            timeTrigger.Pause();
            OnHasTimeLimitChanged?.Invoke(this, false);
        }

        timeTrigger = null;
    }

    public void RegisterPayment(Action? onPaymentFinished, IEnumerable<GoodAmount> goods, int science = 0)
    {
        if (ev is null)
        {
            throw new InvalidOperationException("No active event to register a payment for.");
        }

        if (Payment is not null)
        {
            throw new InvalidOperationException("An active payment is already registered. Clear it before registering a new one.");
        }

        this.onPaymentFinished = onPaymentFinished;

        List<ActiveEventPaymentItem> goodList = [];
        foreach (GoodAmount g in goods)
        {
            if (g.GoodId == ActiveEventPayment.ScienceId)
            {
                science += g.Amount;
            }
            else
            {
                goodList.Add(new(g.GoodId, g.Amount));
            }
        }

        var p = new ActiveEventPayment()
        {
            Science = science > 0 ? new(ActiveEventPayment.ScienceId, science) : null,
            Goods = [.. goodList],
        };

        if (science == 0 && p.Goods.Length == 0)
        {
            throw new ArgumentException("At least one payment item must be specified.");
        }

        Payment = p;
        OnPaymentChanged?.Invoke(this, p);
        eb.Post(new OnNewChroniclePaymentEvent());
    }

    public void RegisterSavedPayment(Action? onPaymentFinished = null)
    {
        if (SavedPayment is null)
        {
            throw new InvalidOperationException("No saved payment to register.");
        }

        if (Payment is not null)
        {
            throw new InvalidOperationException("An active payment is already registered. Clear it before registering a new one.");
        }

        Payment = SavedPayment;
        SavedPayment = null;
        this.onPaymentFinished = onPaymentFinished;

        OnPaymentChanged?.Invoke(this, Payment);
    }

    public bool NeedToPay(string goodId, out int amount)
    {
        amount = 0;
        var p = Payment;
        if (p is null) { return false; }
        return p.NeedToPay(goodId, out amount);
    }

    public bool NeedToPayScience(out int amount) => NeedToPay(ActiveEventPayment.ScienceId, out amount);

    public void Pay(string id, int amount)
    {
        var p = Payment ?? throw new InvalidOperationException("No payment registered.");
        p.Pay(id, amount);

        OnPaymentChanged?.Invoke(this, p);

        if (p.Paid)
        {
            onPaymentFinished?.Invoke();
            onPaymentFinished = null;
        }
    }

    public void PayScience(int amount) => Pay(ActiveEventPayment.ScienceId, amount);

    public void ClearPayment()
    {
        Payment = null;
        OnPaymentChanged?.Invoke(this, null);
        onPaymentFinished = null;
    }

    public void SetActiveDescription(string? description)
    {
        ActiveDescription = description;
        OnActiveDescriptionChanged?.Invoke(this, description);
    }

    public void ClearActiveDescription() => SetActiveDescription(null);

    public void Clear()
    {
        ClearTimeLimit();
        ClearPayment();
        ClearActiveDescription();
    }

    public void ClearSaved()
    {
        SavedProgress = null;
        SavedPayment = null;
    }

    public void Load() => LoadSavedData();

    void LoadSavedData()
    {
        if (!loader.TryGetSingleton(SaveKey, out var s)) { return; }

        if (s.Has(ProgressKey))
        {
            SavedProgress = s.Get(ProgressKey);
        }

        if (s.Has(PaymentKey))
        {
            SavedPayment = ActiveEventPayment.Deserialize(s.Get(PaymentKey));
        }

        if (s.Has(DescriptionKey))
        {
            ActiveDescription = s.Get(DescriptionKey);
        }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
        var s = singletonSaver.GetSingleton(SaveKey);

        if (timeTrigger?.InProgress == true)
        {
            s.Set(ProgressKey, timeTrigger.Progress);
        }

        if (Payment is not null)
        {
            s.Set(PaymentKey, Payment.Serialize());
        }

        if (ActiveDescription is not null)
        {
            s.Set(DescriptionKey, ActiveDescription);
        }
    }

}
