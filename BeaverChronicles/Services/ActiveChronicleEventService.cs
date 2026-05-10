namespace BeaverChronicles.Services;

[BindSingleton]
public class ActiveChronicleEventService(ITimeTriggerFactory timerFac, ISingletonLoader loader) : ISaveableSingleton, ILoadableSingleton
{
    static readonly SingletonKey SaveKey = new(nameof(ActiveChronicleEventService));
    static readonly PropertyKey<float> ProgressKey = new("Progress");
    static readonly PropertyKey<string> PaymentKey = new("Payment");

    public float? SavedProgress { get; private set; }
    public ActiveEventPayment? SavedPayment { get; private set; }

    IChronicleEvent? ev;

    ITimeTrigger? timeTrigger;
    public float RemainingDays => timeTrigger?.DaysLeft ?? 0;
    public bool HasTimeLimit => timeTrigger?.InProgress == true;

    public event EventHandler<IChronicleEvent?>? OnEventChanged;
    public event EventHandler<bool>? OnHasTimeLimitChanged;
    public event EventHandler<ActiveEventPayment?>? OnPaymentChanged;

    Action? onPaymentFinished;

    public ActiveEventPayment? Payment { get; private set; }
    public bool NeedsPayment => Payment?.Paid == false;

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

    public void RegisterTimeLimit(float days, Action action, float? progress = null)
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
        if (progress.HasValue)
        {
            timeTrigger.FastForwardProgress(progress.Value);
        }

        if (HasTimeLimit)
        {
            OnHasTimeLimitChanged?.Invoke(this, true);
        }
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

    public void RegisterPayment(IEnumerable<GoodAmount> goods, int science = 0, Action? onPaymentFinished = null)
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

        var p = new ActiveEventPayment()
        {
            Science = science > 0 ? new(ActiveEventPayment.ScienceId, science) : null,
            Goods = [..goods.Select(g => new ActiveEventPaymentItem(g.GoodId, g.Amount))],
        };

        if (science == 0 && p.Goods.Length == 0)
        {
            throw new ArgumentException("At least one payment item must be specified.");
        }

        Payment = p;
        OnPaymentChanged?.Invoke(this, p);
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

    public void Clear()
    {
        ClearTimeLimit();
        ClearPayment();
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
    }
        
}
