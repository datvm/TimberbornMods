namespace Ziporter.Components;

public class ZiporterConnection : TickableComponent, IFinishedStateListener
{
    public const int ActivateCapacity = 2000;
    public const int DeactivateCapacity = 1000;

    bool active;
    bool finished;

    public event EventHandler? OnActiveChanged;

    public ZiporterConnection? ConnectedZiporter { get; private set; }
    public bool IsConnected => ConnectedZiporter is not null;
    public bool IsActive => IsConnected && active && finished;
    public bool IsFinished => finished;

    static readonly Vector3 CableAnchorPoint = new(1.5f, 3.85f, 1.5f); // From Zipline Tower anchor point
    public Vector3 AnchorPoint => blockObject.Transform(CoordinateSystem.WorldToGrid(CableAnchorPoint));

    #region Reference

#nullable disable
    EntityRegistry entityRegistry;
    ILoc t;
    ZiporterConnectionService service;

    BlockObject blockObject;
    EntityComponent entityComponent;
    ZiporterBattery battery;
#nullable enable

    [Inject]
    public void Inject(
        EntityRegistry entityRegistry,
        ILoc t,
        ZiporterConnectionService service
    )
    {
        this.entityRegistry = entityRegistry;
        this.t = t;
        this.service = service;
    }

    public void Awake()
    {
        entityComponent = GetComponentFast<EntityComponent>();
        blockObject = GetComponentFast<BlockObject>();
        battery = GetComponentFast<ZiporterBattery>();
    }

    #endregion

    public override void Tick()
    {
        if (!finished) { return; }

        var charge = battery.Charge;
        if (active)
        {
            if (charge < DeactivateCapacity)
            {
                ToggleActive(false);
            }
        }
        else if (!active && charge > ActivateCapacity)
        {
            ToggleActive(true);
        }
    }

    public void Load(Guid connectionEntityId)
    {
        var entity = entityRegistry.GetEntity(connectionEntityId);
        if (entity)
        {
            ConnectedZiporter = entity.GetComponentFast<ZiporterConnection>();
        }
        else
        {
            Debug.LogWarning($"ZiporterConnection: Entity with ID {connectionEntityId} not found for connecting with {this}.");
        }
    }

    public void Connect(ZiporterConnection other)
    {
        var validation = ValidateConnection(other);
        if (validation is not null)
        {
            throw new InvalidOperationException(validation);
        }

        ConnectedZiporter = other;
        RaiseActiveChanged();
    }

    public void Disconnect()
    {
        ConnectedZiporter = null;
        RaiseActiveChanged();
    }

    public string? ValidateConnection(ZiporterConnection other)
    {
        if (ConnectedZiporter is not null)
        {
            return "LV.Ziporter.ConnErrAlready".T(t);
        }
        else if (other == entityComponent)
        {
            return "LV.Ziporter.ConnErrSelf".T(t);
        }

        return null;
    }

    public void OnEnterFinishedState()
    {
        finished = true;
        service.Register(this);
        RaiseActiveChanged();
    }

    public void OnExitFinishedState()
    {
        service.Unregister(this);
        RaiseActiveChanged();
    }

    void ToggleActive(bool active)
    {
        this.active = active;
        RaiseActiveChanged();
    }

    void RaiseActiveChanged()
    {
        OnActiveChanged?.Invoke(this, EventArgs.Empty);
    }

}
