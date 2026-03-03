namespace SluiceIsBack.Components;

#nullable disable

public class SluiceState : BaseComponent, IUnfinishedStateListener, IPersistentEntity
{
    static readonly ComponentKey SluiceStateKey = new("SluiceState");

    static readonly PropertyKey<bool> AutoModeKey = new("AutoMode");

    static readonly PropertyKey<bool> IsOpenKey = new("IsOpen");

    static readonly PropertyKey<bool> IsSynchronizedKey = new("IsSynchronized");

    static readonly PropertyKey<bool> AutoCloseOnOutflowKey = new("AutoCloseOnOutflow");

    static readonly PropertyKey<float> OutflowLimitKey = new("OutflowLimit");

    static readonly PropertyKey<bool> AutoCloseOnAboveKey = new("AutoCloseOnAbove");

    static readonly PropertyKey<bool> AutoCloseOnBelowKey = new("AutoCloseOnBelow");

    static readonly PropertyKey<float> OnAboveLimitKey = new("OnAboveLimit");

    static readonly PropertyKey<float> OnBelowLimitKey = new("OnBelowLimit");

    readonly SluiceSynchronizer _sluiceSynchronizer;

    public bool AutoMode { get; private set; } = true;

    public bool IsOpen { get; private set; }

    public bool AutoCloseOnOutflow { get; private set; } = true;

    public float OutflowLimit { get; private set; } = -0.5f;

    public bool AutoCloseOnAbove { get; private set; }

    public bool AutoCloseOnBelow { get; private set; }

    public float OnAboveLimit { get; private set; } = 0.05f;

    public float OnBelowLimit { get; private set; } = 0.05f;

    public bool IsSynchronized { get; private set; } = true;

    public SluiceState(SluiceSynchronizer sluiceSynchronizer)
    {
        _sluiceSynchronizer = sluiceSynchronizer;
    }

    public void SetAuto()
    {
        AutoMode = true;
        Synchronize();
    }

    public void Open()
    {
        IsOpen = true;
        AutoMode = false;
        Synchronize();
    }

    public void Close()
    {
        IsOpen = false;
        AutoMode = false;
        Synchronize();
    }

    public void EnableAutoCloseOnOutflow()
    {
        AutoCloseOnOutflow = true;
        Synchronize();
    }

    public void DisableAutoCloseOnOutflow()
    {
        AutoCloseOnOutflow = false;
        Synchronize();
    }

    public void SetOutflowLimit(float outflowLimit)
    {
        OutflowLimit = outflowLimit;
        Synchronize();
    }

    public void EnableAutoCloseOnAbove()
    {
        AutoCloseOnBelow = false;
        AutoCloseOnAbove = true;
        Synchronize();
    }

    public void DisableAutoCloseOnAbove()
    {
        AutoCloseOnAbove = false;
        Synchronize();
    }

    public void SetAboveContaminationLimit(float contaminationLimit)
    {
        OnAboveLimit = contaminationLimit;
        Synchronize();
    }

    public void EnableAutoCloseOnBelow()
    {
        AutoCloseOnBelow = true;
        AutoCloseOnAbove = false;
        Synchronize();
    }

    public void DisableAutoCloseOnBelow()
    {
        AutoCloseOnBelow = false;
        Synchronize();
    }

    public void SetBelowContaminationLimit(float contaminationLimit)
    {
        OnBelowLimit = contaminationLimit;
        Synchronize();
    }

    public void ToggleSynchronization(bool newValue)
    {
        IsSynchronized = newValue;
        SynchronizeWithNeighbors();
    }

    public void OnEnterUnfinishedState()
    {
        SynchronizeWithNeighbors();
    }

    public void OnExitUnfinishedState()
    {
    }

    public void Save(IEntitySaver entitySaver)
    {
        IObjectSaver component = entitySaver.GetComponent(SluiceStateKey);
        component.Set(AutoModeKey, AutoMode);
        component.Set(IsOpenKey, IsOpen);
        component.Set(OutflowLimitKey, OutflowLimit);
        component.Set(AutoCloseOnOutflowKey, AutoCloseOnOutflow);
        component.Set(AutoCloseOnAboveKey, AutoCloseOnAbove);
        component.Set(AutoCloseOnBelowKey, AutoCloseOnBelow);
        component.Set(OnAboveLimitKey, OnAboveLimit);
        component.Set(OnBelowLimitKey, OnBelowLimit);
        component.Set(IsSynchronizedKey, IsSynchronized);
    }

    public void Load(IEntityLoader entityLoader)
    {
        IObjectLoader component = entityLoader.GetComponent(SluiceStateKey);
        AutoMode = component.Get(AutoModeKey);
        IsOpen = component.Get(IsOpenKey);
        AutoCloseOnOutflow = component.Get(AutoCloseOnOutflowKey);
        OutflowLimit = component.Get(OutflowLimitKey);
        AutoCloseOnAbove = component.Get(AutoCloseOnAboveKey);
        AutoCloseOnBelow = component.Get(AutoCloseOnBelowKey);
        OnAboveLimit = component.Get(OnAboveLimitKey);
        OnBelowLimit = component.Get(OnBelowLimitKey);
        IsSynchronized = component.Get(IsSynchronizedKey);
    }

    public void SetState(SluiceState neighbor, int minLimit)
    {
        AutoMode = neighbor.AutoMode;
        IsOpen = neighbor.IsOpen;
        AutoCloseOnOutflow = neighbor.AutoCloseOnOutflow;
        OutflowLimit = Math.Max(neighbor.OutflowLimit, minLimit);
        AutoCloseOnAbove = neighbor.AutoCloseOnAbove;
        AutoCloseOnBelow = neighbor.AutoCloseOnBelow;
        OnAboveLimit = neighbor.OnAboveLimit;
        OnBelowLimit = neighbor.OnBelowLimit;
        IsSynchronized = neighbor.IsSynchronized;
    }

    public void SetStateAndSynchronize(SluiceState neighbor, int minLimit)
    {
        SetState(neighbor, minLimit);
        Synchronize();
    }

    void SynchronizeWithNeighbors()
    {
        if (IsSynchronized)
        {
            _sluiceSynchronizer.SynchronizeWithNeighbors(this);
        }
    }

    void Synchronize()
    {
        if (IsSynchronized)
        {
            _sluiceSynchronizer.SynchronizeNeighbors(this);
        }
    }
}
