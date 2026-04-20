namespace DistroStorage.Components;

public abstract class DistroComponentBase : BaseComponent, IDistroComponent, IPreviewStateListener, IPersistentEntity, IAwakableComponent, IFinishedStateListener, IInitializableEntity, IDeletableEntity
{
    static readonly ComponentKey SaveKey = new(nameof(DistroComponentBase));
    static readonly PropertyKey<bool> EnabledKey = new(nameof(Enabled));

    protected BlockObject blockObject = null!;

    bool disabledBySettings;
    public virtual bool DisabledBySetting => disabledBySettings;
    public virtual bool EnabledByDefault => true;
    public virtual bool RequireFinishedBuilding => true;

    public IDistroReceiver? Receiver { get; }
    public bool IsReceiver { get; }

    public IDistroSender? Sender { get; }
    public bool IsSender { get; }

    bool active = false, activeDirty = true;
    public bool Active
    {
        get
        {
            if (activeDirty)
            {
                active = CalculateActive();
                activeDirty = false;
            }
            return active;
        }
    }

    public bool ActiveAndEnabled => Enabled && Active; // Typically Enabled is faster

    public BoundsInt Bounds { get; protected set; }
    public abstract IEnumerable<GoodAmount> Goods { get; }
    public abstract IEnumerable<string> GoodIds { get; }

    readonly HashSet<IDistroComponent> connectedComponents = [];
    protected readonly DistroService service;

    public DistroComponentBase(DistroService service)
    {
        this.service = service;

        Receiver = this as IDistroReceiver;
        Sender = this as IDistroSender;
        IsReceiver = Receiver is not null;
        IsSender = Sender is not null;

        if (IsReceiver && IsSender)
        {
            throw new InvalidOperationException($"Component {this} cannot be both a sender and receiver.");
        }
        else if (!IsReceiver && !IsSender)
        {
            throw new InvalidOperationException($"Component {this} must be either a sender or receiver.");
        }
    }

    public IReadOnlyCollection<IDistroComponent> Connections => connectedComponents;

    public void SetEnabled(bool enabled)
    {
        if (enabled)
        {
            EnableComponent();
        }
        else
        {
            DisableComponent();
        }

        MarkActiveDirty();
    }

    protected void MarkActiveDirty() => activeDirty = true;

    protected virtual bool CalculateActive() =>
        !DisabledBySetting
        && (!RequireFinishedBuilding || blockObject.IsFinished);

    public virtual void DeleteEntity()
    {
        service.Unregister(this);
    }

    public virtual void InitializeEntity()
    {
        Bounds = blockObject.GetBounds();
        service.Register(this);
    }

    public void ConnectWith(IDistroComponent component)
    {
        if (IsReceiver == component.IsReceiver)
        {
            throw new InvalidOperationException($"Cannot connect component {this} to {component} because they are both {(IsReceiver ? "receivers" : "senders")}.");
        }

        if (!connectedComponents.Add(component))
        {
            throw new InvalidOperationException($"Component {this} is already connected to {component}.");
        }
    }

    public void DisconnectFrom(IDistroComponent component)
    {
        // Do not validate removal
        connectedComponents.Remove(component);
    }

    public void ClearConnections()
    {
        connectedComponents.Clear();
    }

    public void OnEnterPreviewState()
    {
        disabledBySettings = true;
        DisableComponent();
        MarkActiveDirty();

        service.Unregister(this);
    }

    public virtual void Awake()
    {
        blockObject = GetComponent<BlockObject>();

        if (!EnabledByDefault)
        {
            DisableComponent();
        }
    }

    public virtual void Save(IEntitySaver entitySaver)
    {
        var s = entitySaver.GetComponent(SaveKey);
        s.Set(EnabledKey, Enabled);
    }

    public virtual void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        var shouldEnable = s.Get(EnabledKey);
        if (shouldEnable != Enabled)
        {
            SetEnabled(shouldEnable);
        }
    }

    public virtual void OnEnterFinishedState() => MarkActiveDirty();
    public virtual void OnExitFinishedState() => MarkActiveDirty();
}
