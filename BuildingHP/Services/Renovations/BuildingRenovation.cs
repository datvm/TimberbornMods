namespace BuildingHP.Services.Renovations;

public class BuildingRenovationDependencies(
    ILoc T,
    DialogBoxShower DialogBoxShower
)
{
    public readonly ILoc T = T;
    public readonly DialogBoxShower DialogBoxShower = DialogBoxShower;
}

public class BuildingRenovation(BuildingRenovationComponent building, RenovationSpec spec, BuildingRenovationDependencies di)
{
    protected readonly BuildingRenovationDependencies di = di;

    public string Id => Spec.Id;

    public BuildingRenovationComponent Building { get; protected set; } = building;
    public RenovationSpec Spec { get; protected set; } = spec;
    public IReadOnlyList<GoodAmountSpecNew> Cost { get; protected set; } = spec.Cost;
    public float? Days { get; protected set; } = spec.Days;
    public ITimeTrigger? TimeTrigger { get; set; }

    public event Action? GoodAcquireFailed;
    public event Action? GoodAcquired;
    public event Action? Completed;
    public event Action? Canceled;

    /// <summary>
    /// Fired when the renovation is either completed or canceled.
    /// </summary>
    public event Action? Done;

    public bool InProgress => TimeTrigger?.InProgress == true;
    public bool IsGoodAcquired { get; protected set; }
    public bool IsCompleted { get; protected set; }
    public bool IsCanceled { get; protected set; }
    public bool IsDone => IsCompleted || IsCanceled;

    public bool CanCancel { get; protected set; } = !spec.CannotCancel;

    protected bool AddToActiveListOnComplete { get; set; } = true;

    public void OnGoodAcquireFailed()
    {
        if (IsGoodAcquired)
        {
            throw new InvalidOperationException("Goods already acquired");
        }
        ThrowIfDone();

        GoodAcquireFailed?.Invoke();
    }

    public virtual void OnGoodAcquired(ITimeTrigger? timeTrigger)
    {
        if (IsGoodAcquired)
        {
            throw new InvalidOperationException("Goods already acquired");
        }
        ThrowIfDone();

        IsGoodAcquired = true;
        if (timeTrigger is not null)
        {
            TimeTrigger = timeTrigger;
            timeTrigger.Resume();
        }

        ProcessOnGoodAcquired();
        GoodAcquired?.Invoke();

        if (timeTrigger is null)
        {
            OnCompleted();
        }
    }

    public void FinishNow()
    {
        if (IsDone) { return; }

        if (!IsGoodAcquired)
        {
            OnGoodAcquired(null);
            // Because the time trigger is null, it's also completed now, don't call OnCompleted again.
        }
        else
        {
            if (TimeTrigger is not null)
            {
                TimeTrigger.FastForwardProgress(1);
            }
            else
            {
                OnCompleted();
            }
        }
    }

    public virtual void OnCompleted()
    {
        ThrowIfDone();

        IsCompleted = true;
        ProcessOnCompleted();
        Completed?.Invoke();
        OnDone();
    }

    public void RequestCancel()
    {
        ThrowIfDone();
        ProcessCancelRequest();
    }

    public virtual void PerformCancel()
    {
        ThrowIfDone();
        IsCanceled = true;
        Canceled?.Invoke();
        OnDone();
    }

    void OnDone()
    {
        Done?.Invoke();

        if (TimeTrigger is not null)
        {
            TimeTrigger.Pause();
            TimeTrigger = null;
        }
    }

    protected virtual void ProcessCancelRequest()
    {
        var t = di.T;

        di.DialogBoxShower.Create()
            .SetLocalizedMessage("LV.BHP.CancelRenoConfirm")
            .SetConfirmButton(TimberUiUtils.DoNothing, t.T("LV.BHP.CancelNo"))
            .SetCancelButton(PerformCancel, t.T("LV.BHP.CancelRenoYes"))
            .Show();
    }

    void ThrowIfDone()
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException("Renovation already completed");
        }

        if (IsCanceled)
        {
            throw new InvalidOperationException("Renovation already canceled");
        }
    }

    protected virtual void ProcessOnGoodAcquired() { }

    protected virtual void ProcessOnCompleted()
    {
        Building.AddCompletedRenovation(Spec.Id, AddToActiveListOnComplete);
    }

}
